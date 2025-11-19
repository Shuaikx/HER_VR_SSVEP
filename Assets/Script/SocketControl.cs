using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SocketControl : MonoBehaviour
{
    public string ServerIP = "127.0.0.1";
    public Int32 ServerPort = 4003;

    [SerializeField]
    private bool TestScene;

    [SerializeField]
    private bool SSVEPScene;
    private TcpClient clientSocket;
    private NetworkStream stream;
    private byte[] receiveBuffer = Encoding.Default.GetBytes("Hello");
    public static bool authenticateState = false;

    [Tooltip("???Task1 ?? SSVEP ??????????")]
    public aimTarget_SSEVEP aimTarget;

    [Tooltip("???Task3 ?? SSVEP ??????????")]
    //public aimTarget_SSVEP_t2 aimTarget_t2;
    public TestSceneControl testControl;

    [SerializeField]
    private bool noConnectedWithEEG;

    //public FlickerControl flickerControl;
    // public ControlManager manager;

    // ????????????????? Stopwatch
    private Stopwatch _authenticationStopwatch;

    // ??????????????????????
    [Tooltip("???????????????????????????????????????")]
    public float networkLatencyMs = 0f;

    private void Start()
    {
        Loom.Initialize();
        if (noConnectedWithEEG)
        {
            SocketControl.authenticateState = true; // only for test
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !authenticateState)
        {
            StartClient();
        }
        if (Input.GetKeyDown(KeyCode.E) && authenticateState)
        {
            SendQuitMessage();
            Application.Quit();
        }
    }

    public void SendQuitMessage()
    {
        byte[] message = new byte[1] { 0xFF };
        SendDataToServer(message);
    }

    public void StartClient()
    {
        if (SSVEPScene)
            FlickerControl.Instance.SetTextBoardContext("Connecting");
        ConnectToServer();
    }

    public void EndClient()
    {
        if (stream != null)
            stream.Close();
        if (clientSocket != null)
        {
            clientSocket.Close();
            Debug.Log("TCP Client already closed.");
        }
        authenticateState = false;
        // manager.ChangeCurrentConnection(1);
    }

    public void ConnectToServer()
    {
        try
        {
            clientSocket = new TcpClient(ServerIP, ServerPort);
            stream = clientSocket.GetStream();
            Debug.Log("Connected to server.");
            SendAuthentication();
            StartReceiving();
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }

    public void SendAuthentication()
    {
        byte[] message = new byte[1] { 0x11 };
        _authenticationStopwatch = Stopwatch.StartNew();
        SendDataToServer(message);
        Debug.Log("Sent Authentication Message");
    }

    private void StartReceiving()
    {
        stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnReceiveData, null);
    }

    private void OnReceiveData(IAsyncResult result)
    {
        try
        {
            int bytesRead = stream.EndRead(result);
            if (bytesRead > 0)
            {
                // Debug.Log("Target Type: "+result.GetType()+", Result Length: "+bytesRead);
                // string receivedMessage = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                Debug.Log("Received from server: " + receiveBuffer[0].ToString());
                Loom.QueueOnMainThread(
                    (param) =>
                    {
                        // manager.OnMessageReceived(receivedMessage);
                        if (!authenticateState)
                            Authenticate(receiveBuffer);
                        else
                        {
                            if (SSVEPScene)
                                FlickerControl.Instance.HandleResult(receiveBuffer); // For SSVEP
                            if (TestScene)
                                TestSceneControl.HandleResult(receiveBuffer); // For test
                        }
                    },
                    null
                );
            }

            StartReceiving(); //
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    private void Authenticate(byte[] receivedMessage)
    {
        if (receivedMessage[0] == 0x11)
        {
            authenticateState = true;
            if (_authenticationStopwatch != null && _authenticationStopwatch.IsRunning)
            {
                _authenticationStopwatch.Stop();
                networkLatencyMs = (float)_authenticationStopwatch.Elapsed.TotalMilliseconds;
                UnityEngine.Debug.Log(
                    $"The server is authenticated. Network Latency: {networkLatencyMs:F2} ms"
                );
            }
            else
            {
                UnityEngine.Debug.Log(
                    "The server is authenticated, but latency could not be measured."
                );
            }

            if (SSVEPScene)
                FlickerControl.Instance.SetTextBoardContext("Press Space when you are ready.");
        }
        else
        {
            Debug.Log("The server is not authenticated!");
        }
    }

    public static bool GetAuthenticateState()
    {
        return authenticateState;
    }

    public void SendDataToServer(string message)
    {
        try
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(messageBytes, 0, messageBytes.Length);
            stream.Flush(); //
            Debug.Log("Data " + message + " sent to server");
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data to server: " + e.Message);
        }
    }

    public void SendDataToServer(byte[] message)
    {
        try
        {
            stream.Write(message, 0, message.Length);
            stream.Flush(); //
            Debug.Log("Data " + message[0].ToString() + " sent to server");
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending data to server: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        EndClient();
        if (clientSocket != null)
            clientSocket.Dispose();
    }
}
