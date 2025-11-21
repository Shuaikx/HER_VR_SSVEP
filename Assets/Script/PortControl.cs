using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PortControl : MonoBehaviour
{
    #region Serial Port Parameters
    public string portName = "COM12"; // Port name
    public int baudRate = 115200 * 4; // Baud rate
    public Parity parity = Parity.None; // Parity bit
    public int dataBits = 8; // Data bits
    public StopBits stopBits = StopBits.One; // Stop bits
    SerialPort sp = null;
    Thread dataReceiveThread;

    //���͵���Ϣ
    public List<byte> listReceive = new List<byte>();
    char[] strchar = new char[100]; //���յ��ַ���Ϣת��Ϊ�ַ�������Ϣ
    string str;
    #endregion
    void Start()
    {
        baudRate = 115200 * 4;
        //baudRate = 115200 * 1;
        OpenPort();
        // �ڱ���ʵ���У�ֻ�漰���̼�����Ҫ���Ե�װ�ô�trigger ����Ҫ����trigger����˰�Port�Ľ�����Ϣ�Ĺ���ȥ��
        //dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
        //dataReceiveThread.Start();
    }

    #region Open serial port, or open port
    public void OpenPort()
    {
        // Set parameters
        sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        sp.ReadTimeout = 400;
        try
        {
            sp.Open();
            Debug.Log($"Start the {portName} port");
        }
        catch (Exception ex)
        {
            Debug.Log("Port open failed: " + ex.Message);
        }
    }
    #endregion


    #region �����˳�ʱ�رմ���
    void OnApplicationQuit()
    {
        ClosePort();
    }

    public void ClosePort()
    {
        try
        {
            sp.Close();
            dataReceiveThread.Abort();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion


    /// <summary>
    /// ��ӡ���յ���Ϣ
    /// </summary>
    void PrintData()
    {
        for (int i = 0; i < listReceive.Count; i++)
        {
            strchar[i] = (char)(listReceive[i]);
            str = new string(strchar);
        }
        Debug.Log(str);
    }

    #region ��������
    void DataReceiveFunction()
    {
        #region ���ֽ����鷢�ʹ�����Ϣ����Ϣȱʧ
        byte[] buffer = new byte[1024];
        int bytes = 0;
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    bytes = sp.Read(buffer, 0, buffer.Length); // Read bytes
                    if (bytes == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string strbytes = Encoding.Default.GetString(buffer);
                        Debug.Log(strbytes);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(ThreadAbortException)) { }
                }
            }
            Thread.Sleep(10);
        }
        #endregion
    }
    #endregion


    #region Send data
    public void WriteData(byte[] data)
    {
        if (sp.IsOpen)
        {
            // Debug.LogWarning($"Start Trigger {Time.realtimeSinceStartup*1000}");
            sp.Write(data, 0, data.Length);
            // var endTime = Time.realtimeSinceStartup - startTime;
            // Debug.LogWarning($"End Trigger{Time.realtimeSinceStartup*1000}");
        }
    }

    public void WriteData(string data)
    {
        if (sp.IsOpen)
        {
            sp.WriteLine(data);
        }
    }

    #endregion
}
