using System;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class FlickerControl : MonoBehaviour
{
    [SerializeField] PortControl port;
    [SerializeField] SocketControl socket;
    [SerializeField] aimTarget_SSEVEP aim;
    [SerializeField] private float notifyTime = 1f;
    [SerializeField] private float flickTime = 4f;
    [SerializeField] private float reponseTime = 1f;
    [SerializeField] private float timer = 0f;

    private int cueCount => Mathf.RoundToInt(notifyTime * 60);
    private int flickCount => Mathf.RoundToInt(flickTime * 60);
    private int responseCount => Mathf.RoundToInt(reponseTime * 60);
    private int countIdx = 0;
    private int totalCount => cueCount + flickCount + responseCount;
    [SerializeField] private GameObject CurrentBall;
    [SerializeField] private Text TextBoard;
    [SerializeField] private DataRecorder dataRecorder;
    private static Ball_SSVEP ball;
    private Outline outline;
    private bool flickerState = false;
    private bool isStimulated = false;
    private bool socketDataSent = false;
    private bool pause = false;
    private bool isStimulatEnd = false;
    public static FlickerControl Instance { get; private set; }

    // TurnFlickerOn 事件：当闪烁开启时触发，并传递当前闪烁的GameObject
    public event Action OnFlickerTurnedOn;
    // TurnFlickerOff 事件：当闪烁关闭时触发
    public event Action OnFlickerTurnedOff;

    private void Awake()
    {
        // 实现单例模式逻辑
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("FlickerControl: Found an existing instance, destroying new one.", this);
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 如果你希望这个单例在场景切换时不被销毁，添加以下行
            // DontDestroyOnLoad(gameObject); // 根据你的需求决定是否需要
        }
        
        ball = CurrentBall.GetComponent<Ball_SSVEP>();
        outline = CurrentBall.GetComponent<Outline>();
        dataRecorder.CreateNewLogFile();
    }

    // private void Start()
    // {
    //    
    // }
    private byte[] trigger;
    private byte[] triggerEnd;

    private void Update()
    {
        // Debug.LogWarning($" {Time.realtimeSinceStartup * 1000} - {countIdx}");

        if (SocketControl.GetAuthenticateState() && flickerState && countIdx < totalCount)
        {

            // Cue
            if (countIdx < cueCount - 1)
            {

                outline.OutlineColor = Color.blue;
                outline.enabled = true;
            }
            else if (countIdx == cueCount - 1)
            {
                outline.enabled = false;
                byte b = Convert.ToByte(ball.Index);
                trigger = new byte[1] { b };
                byte b1 = Convert.ToByte(200);
                triggerEnd = new byte[1] { b1 };
                // Debug.LogWarning($"Cue End {Time.realtimeSinceStartup * 1000} - {countIdx}");

            }
            // Stim
            else if (countIdx >= cueCount && countIdx < cueCount + flickCount)
            {
                
                // Debug.LogWarning(Time.frameCount);
                // 刺激第一帧
                if (!isStimulated)
                {
                    // Debug.LogWarning($"Stim 1 {Time.realtimeSinceStartup * 1000}");

                    port.WriteData(trigger);
                    // Debug.Log($"Trigger {trigger[0]} is sent");
                    isStimulated = true;
                    // gameObject.BroadcastMessage("startStimulate");
                    OnFlickerTurnedOn?.Invoke();
                    // Debug.LogWarning("OnFlickerTurnedOn Invoked");

                }
                
                
            }
            // 反馈
            else if (countIdx >= cueCount + flickCount )
            {
                if (countIdx == cueCount + flickCount)
                {
                    if (!isStimulatEnd)
                    {
                        // Debug.LogWarning($"Stim 1 {Time.realtimeSinceStartup * 1000}");

                        port.WriteData(triggerEnd);
                        // Debug.Log($"Trigger {trigger[0]} is sent");
                        isStimulatEnd = true;
                    }
                }
                if (!socketDataSent)
                {
                    byte b = System.Convert.ToByte(ball.Index);
                    byte[] target = new byte[1] { b };
                    socket.SendDataToServer(target);
                    socketDataSent = true;
                }

                if (isStimulated)
                {
                    // gameObject.BroadcastMessage("endStimulate");
                    OnFlickerTurnedOff?.Invoke();
                    isStimulated = false;

                }

            }
            
            countIdx++;

        }
        else
        {
            socketDataSent = false;
            timer = 0f;
            flickerState = false;
            isStimulatEnd = false;
            countIdx = 0;
        }



    }

    public void TurnFlickerOn(GameObject targetBall)
    {

        // ball.endStimulate();
        // trialIndex += 1;
        gameObject.BroadcastMessage("endStimulate");
        CurrentBall = targetBall;
        ball = CurrentBall.GetComponent<Ball_SSVEP>();
        outline = CurrentBall.GetComponent<Outline>();
        if (!flickerState)
            flickerState = true;

    }
    public void TurnFlickerOff()
    {
        if (flickerState)
            flickerState = false;
        gameObject.BroadcastMessage("endStimulate");
    }

    public void HandleResult(byte[] resultMessage)
    {
        if (System.Convert.ToByte(254) == resultMessage[0])
        {
            Debug.Log($"Received: Saving the model, pause!");
            SetTextBoardContext("Saving...");
            pause = true;
            return;
        }
        if (System.Convert.ToByte(253) == resultMessage[0])
        {
            Debug.Log($"Received: Saving sucess, the stimulus will start in 3 seconds");
            pause = false;
            Invoke("toNextTarget", 3.0f);
            //SetTextBoardContext("Practice Phase");
            return;
        }
        bool result = System.Convert.ToByte(ball.Index) == resultMessage[0];
        if (result)
        {
            Debug.Log($"Received: Ball {ball.Index}'s result is Correct!");
            outline.OutlineColor = Color.green;
            outline.enabled = true;
            Invoke("toNextTarget", 2.0f);
        }
        else
        {
            Debug.Log($"Received: Ball {ball.Index}'s result is Incorrect!");
            outline.OutlineColor = Color.red;
            outline.enabled = true;
            Invoke("toNextTarget", 2.0f);
        }

        dataRecorder.LogDataRow(aim.getBlockIndex(), ball.Index, result, 3.0f);
    }
    private void toNextTarget()
    {
        if (pause)
            return;
        //Debug.Log("To next target");
        outline.enabled = false;
        if (aim)
            aim.showNextTarget();
        else
            return;
    }

    public void SetTextBoardContext(string context)
    {
        TextBoard.text = context;
    }
}
