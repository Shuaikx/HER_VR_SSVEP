using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Oculus.Platform.Models;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class aimTarget_SSEVEP : MonoBehaviour
{
    private int trialCount;
    private GameObject currentBall;
    public sceneManager sM;

    //private bool startGame = false;
    private int targetNumber;

    [SerializeField]
    private int currentTargetIndex;

    [SerializeField]
    private int count = 0;

    [SerializeField]
    private bool inPractice = true;

    [SerializeField]
    private int practiceBlockCount = 2;

    [SerializeField]
    private int maxPracticeBlockCount = 5;

    [SerializeField]
    private SocketControl socketControl;

    //[SerializeField] private Text textBoard;
    private bool isPause = true;

    // Use this for initialization
    protected virtual void Start()
    {
        trialCount = IOManager.GetTrialCount();
        targetNumber = sceneManager.Instance.targetNumber;

        FlickerControl.Instance.SetTextBoardContext("Press C to Connect.");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!SocketControl.GetAuthenticateState())
        {
            return;
        }

        if (isPause)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (count == trialCount)
                {
                    if (socketControl)
                    {
                        socketControl.SendQuitMessage();
                    }
                    sceneManager.QuitApplication();
                }
                // SetTextBoardContext("Pratice Block");
                isPause = false;
                initTargetIndex();
            }
            return;
        }

        if (inPractice) // 训练阶段
        {
            if (count == maxPracticeBlockCount * targetNumber)
                sceneManager.QuitApplication();

            if (Input.GetKeyDown(KeyCode.N))
            {
                FlickerControl.Instance.TurnFlickerOff();
                showNextTarget();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                FlickerControl.Instance.TurnFlickerOff();
                showNextTarget();
                //Debug.Log("Trial: " + count.ToString());
            }
        }
    }

    public int getCount()
    {
        return count;
    }

    public int getBlockIndex()
    {
        return (int)Mathf.Floor((count - 1) / 11) + 1;
    }

    protected virtual void clabiratedAndStart()
    {
        FlickerControl.Instance.TurnFlickerOff();
        inPractice = false;
        sM.setIsPratice(false);
        isPause = true;
        count = 0;
        currentTargetIndex = 0;
        FlickerControl.Instance.SetTextBoardContext("Press Space when you are ready.");
        //initTargetIndex();
    }

    public void showNextTarget()
    {
        Debug.Log($"Count: {count}, currentTargetIndex: {currentTargetIndex}");
        if (inPractice)
            FlickerControl.Instance.SetTextBoardContext("Practice Block");
        else
            FlickerControl.Instance.SetTextBoardContext(
                "Formal Block " + (Mathf.Floor(count / 11) + 1).ToString()
            );

        currentTargetIndex++;
        // Debug.Log("Count: " + count);

        if (currentTargetIndex >= targetNumber)
        {
            UpdateScene();

            if (inPractice && count >= practiceBlockCount * targetNumber)
            {
                Debug.Log("Start the game");
                clabiratedAndStart();
                return;
            }

            FlickerControl.Instance.SetTextBoardContext("Press Space when you are ready.");
            isPause = true;
            currentTargetIndex = 0;
            return;
        }

        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        FlickerControl.Instance.TurnFlickerOn(currentBall);
        count++;
    }

    void initTargetIndex()
    {
        currentTargetIndex = 0;
        if (inPractice)
            FlickerControl.Instance.SetTextBoardContext("Practice Block");
        else
            FlickerControl.Instance.SetTextBoardContext(
                "Formal Block " + (Mathf.Floor((count + 1) / 11) + 1).ToString()
            );
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        FlickerControl.Instance.TurnFlickerOn(currentBall);
        count++;
    }

    public void initPracticeSetUp()
    {
        count = 0;
        FlickerControl.Instance.SetTextBoardContext("Practice Block");
        initTargetIndex();
    }

    protected virtual void UpdateScene()
    {
        sM.updateScene();
    }
}
