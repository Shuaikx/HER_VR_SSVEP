using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;

public class T2_aimTarget_SSEVEP : MonoBehaviour
{
    private int trialCount;
    public IOManager ioM;
    public FlickerControl flickerControl;
    public GameObject Text;
    private Text TitleText;
    private Ball_SSVEP ball;
    private Ball_SSVEP preBall;
    private GameObject currentBall;
    public recordData recData;
    private float currentTime;
    private bool updating = true;
    bool startGame = false;
    
    public int currentTargetIndex;
    public int count = 0;
    private bool isConfirmed_once = false;
    public static bool inPractice = true;
    private OVRInput.Button buttonA = OVRInput.Button.One;  // 默认是 A 按钮
    private OVRInput.Button buttonB = OVRInput.Button.Two;  // 默认是 B 按钮
    public int PracticeBlockNumber = 5;
    private bool isPause = false;
    private int targetsNumber;

    
    // Use this for initialization
    void Start()
    {
        trialCount = IOManager.GetTrialCount();
        // initTargetIndex();
        TitleText = Text.GetComponent<Text>();
        TitleText.text = "Practice Phase";
        targetsNumber = T2_SceneManager.GetTargetNumber();
        // SocketControl.authenticateState = true; //only for test;
    }


    void Update()
    {
        if (isPause)
        {
            if ( Input.GetKeyDown(KeyCode.Space))
            {
                SetTextBoardContext("Pratice Block");
                isPause = false;
                initTargetIndex();
            }
            return;
        }
        confirmationMethods();

        if (inPractice) // 训练阶段
        {
            if(count == PracticeBlockNumber * targetsNumber)
            {
                sceneManager.QuitApplication();
            }
            if (isConfirmed_once || Input.GetKeyDown(KeyCode.N) || OVRInput.GetDown(buttonB))
            {
                flickerControl.TurnFlickerOff();
                showNextTarget();
            }
            if (Input.GetKeyDown(KeyCode.Z) || OVRInput.GetDown(buttonA))
             {
                flickerControl.TurnFlickerOff();
                initTargetIndex();
                inPractice = false; // 跳出训练阶段
                TitleText.text = "Clabirate Phase";
                count = 0;
             }
        }

        if(!inPractice)
            clabiratedAndStart(); //校准

        if ((isConfirmed_once || Input.GetKeyDown(KeyCode.S) || OVRInput.GetDown(buttonB)) && !inPractice)
        {
            if (startGame)
            {
                if (!updating)
                    updating = true;
                reSetData();

                // ball.endNotify();
                showNextTarget();
                Debug.Log("Trial: " + count.ToString());
                if(count == trialCount)
                {
                    IOManager.finishExperiment();
                    SceneManager.LoadScene("over");
                }
                TitleText.text = (Mathf.Floor((99 - count)  / 11)).ToString();
            }
        }

    }


    void clabiratedAndStart()
    {
        if (!startGame)
        {
            if (Input.GetKeyDown("z") || OVRInput.GetDown(buttonA))
            {
                preBall = null;
                startGame = true;
                currentTime = Time.time;
                
                if (IOManager.stateCM == Modality.ModalityState.SSVEP)
                {
                   IOManager.createUser_SSVEP();
                }
            }
        }
    }

    public void showNextTarget()
    {
        currentTargetIndex++;
        if (currentTargetIndex == targetsNumber)
        {
            SetTextBoardContext("Press Space when you are ready.");
            isPause = true;
            currentTargetIndex = 0;
            return;
        }
        count++;
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        flickerControl.TurnFlickerOn(currentBall);
    }

    void initTargetIndex()
    {
        currentTargetIndex = 0;
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        flickerControl.TurnFlickerOn(currentBall);
        count++;
    }

    public void initTestSetUp() {
        TitleText.text = "Practice Phase";
        initTargetIndex();
    } 

    void reSetData()
    {
        currentTime = Time.time;
        preBall = ball;
    }


    void confirmationMethods()
    {
        //isConfirmed_once = false;
        //if (IOManager.stateCM == Modality.ModalityState.SSVEP)
        //{
        //    SSVEP();
        //}
        
    }

    private void SetTextBoardContext(string text)
    {

    }

}
