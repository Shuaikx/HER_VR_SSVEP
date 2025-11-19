using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using UnityEngine;

public class TestSceneControl : MonoBehaviour
{
    [SerializeField]
    PortControl port;

    [SerializeField]
    SocketControl socket;

    [SerializeField]
    private float notifyTime = 1f;

    [SerializeField]
    private float flickTime = 4f;

    [SerializeField]
    private float reponseTime = 1f;

    [SerializeField]
    private float timer = 0f;

    [SerializeField]
    private GameObject Ball;
    private static Ball_SSVEP SSVEP;
    private static bool flickerState = false;
    private bool portDataSent = false;
    private bool socketDataSent = false;
    private int trialIndex = 0;

    private void Start()
    {
        SSVEP = Ball.GetComponent<Ball_SSVEP>();
        Application.targetFrameRate = 60;
        /*byte b = System.Convert.ToByte(14);
        byte[] testMessage = new byte[1] { b };
        int result = System.BitConverter.ToInt32(testMessage, 0);
        Debug.Log($"The test message is {result}");*/
    }

    void Update()
    {
        if (SocketControl.GetAuthenticateState() && flickerState)
        {
            if (!socketDataSent)
            {
                byte b = System.Convert.ToByte(SSVEP.Index);
                byte[] target = new byte[1] { b };
                socket.SendDataToServer(target);
                socketDataSent = true;
            }

            timer += Time.deltaTime;

            if (timer <= notifyTime)
            {
                SSVEP.startNotify();
            }
            else if (timer > notifyTime && timer <= notifyTime + flickTime)
            {
                if (!portDataSent)
                {
                    byte b = System.Convert.ToByte(SSVEP.Index);
                    byte[] trigger = new byte[1] { b };
                    Debug.Log("Trigger: " + trigger[0].ToString());
                    port.WriteData(trigger);
                    Debug.Log("Trigger is sent");
                    portDataSent = true;
                }
                SSVEP.startStimulate();
            }
            else if (
                timer > notifyTime + flickTime
                && timer <= notifyTime + flickTime + reponseTime
            )
            {
                SSVEP.endStimulate();
            }
            else
            {
                portDataSent = false;
                socketDataSent = false;
                timer = 0f;
                flickerState = false;
            }
        }
        //else
        //{
        //    ball.endStimulate();
        //}
    }

    public void TurnFlickerOn()
    {
        if (!flickerState)
            flickerState = true;
        trialIndex += 1;
        Debug.Log("Flicker on");
    }

    public void TurnFlickerOff()
    {
        if (flickerState)
            flickerState = false;
        //ball = null;
        Debug.Log("Flicker off");
    }

    public static void HandleResult(byte[] resultMessage)
    {
        // int result = System.BitConverter.ToInt32(resultMessage, 0);
        // Debug.Log($"Ball {ball.Index}'s result is {result}");
        if (System.Convert.ToByte(SSVEP.Index) == resultMessage[0])
        {
            Debug.Log($"Ball {SSVEP.Index}'s result is Correct!");
        }
        else
        {
            Debug.Log($"Warnign: Ball {SSVEP.Index}'s result is Incorrect!");
        }
    }

    public static void ShowOutline()
    {
        Outline outline = SSVEP.GetComponent<Outline>();
        outline.enabled = true;
    }
}
