using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Don't listen in update, directly write the logic to jump to next target into a function, trigger after completing logic.
/// </summary>
public class aimTarget_Gaze : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField]
    private float notifyTime = 1f;

    [SerializeField]
    private float flickTime = 4f;

    [SerializeField]
    private float reponseTime = 1f;

    [Header("Gaze Settings")]
    [SerializeField]
    private GameObject cursor;

    [SerializeField]
    private GameObject leftEyePos;

    [SerializeField]
    private GameObject rightEyePos;

    [SerializeField]
    private GameObject cam;

    [SerializeField]
    private GameObject centerPoint;

    [SerializeField]
    private Text textBoard;

    [Header("Scene Settings")]
    [SerializeField]
    private AudioClip bingo;

    [SerializeField]
    private recordData recData;

    [SerializeField]
    private sceneManager sM;

    [SerializeField]
    private int currentTargetIndex;

    [SerializeField]
    private int count = 0;

    [SerializeField]
    private int maxPracticeBlockCount = 2;

    [SerializeField]
    private DataRecorder dataRecorder;

    private GameObject currentBall;
    private float currentTime;
    private Ray combinedRay;
    protected bool updating = false;
    public int targetNumber;

    private OVRInput.Button buttonA = OVRInput.Button.One; // 默认是 A 按钮
    private OVRInput.Button buttonB = OVRInput.Button.Two; // 默认是 B 按钮

    [Header("Ball Controller")]
    [SerializeField]
    private GazeBallsController ballsController;

    private bool isConfirmed_dwell = false;
    private float dwellTimer_dwell = 0f;
    private Outline ballOutline = null;
    private Outline targetOutline = null;

    [SerializeField]
    private float timer;

    [SerializeField]
    private bool isDwellable = true;

    [SerializeField]
    private bool isPause = true;

    [SerializeField]
    private bool isConfirmed = false;
    private int trialCount;
    private GazeStatus gazeStatus;

    [SerializeField]
    public float cursorDistance = 97f;

    enum GazeStatus
    {
        Practice,
        Formal,
    }

    // Use this for initialization
    protected virtual void Start()
    {
        trialCount = IOManager.GetTrialCount();
        if (sceneManager.Instance)
            targetNumber = sceneManager.Instance.targetNumber;
        else
            targetNumber = 11;
        SetTextBoardContext("Press Space to Start the Practice Blocks");
        initCurosrVisibility(); // Decide cursor display
        isPause = true;
        dataRecorder.CreateNewLogFile();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isPause)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (count == trialCount)
                {
                    IOManager.finishExperiment();
                    sceneManager.QuitApplication();
                }
                SetTextBoardContext(
                    gazeStatus.ToString() + " Block " + (Mathf.Floor((count) / 11) + 1).ToString()
                );
                isPause = false;
                resetData();
                initTargetIndex();
                isDwellable = true;
            }
            return;
        }
        confirmationMethods();

        defaultProcess(); // Order cannot change

        if (gazeStatus == GazeStatus.Practice) // Training phase
        {
            if (
                Input.GetKeyDown(KeyCode.Z)
                || OVRInput.GetDown(buttonA)
                || count == maxPracticeBlockCount * targetNumber + 1
            )
            {
                clabiratedAndStart(); // Calibrate
                return;
            }
        }

        // updateScene();
    }

    protected virtual void clabiratedAndStart()
    {
        sM.setIsPratice(false);

        gazeStatus = GazeStatus.Formal;
        SetTextBoardContext("Press Space to Start the Formal Blocks");
        if (sM)
            sM.updateOriginSence();
        resetData();
        isPause = true;
        initTargetIndex();
        count = 0;
    }

    private void defaultProcess()
    {
        if (isPause)
            return;

        if (isConfirmed)
            return;

        timer += Time.deltaTime;

        if (timer < notifyTime)
        {
            if (isDwellable)
            {
                targetOutline.OutlineColor = Color.blue;
                targetOutline.OutlineWidth = 5f;
                targetOutline.enabled = true;
                isDwellable = false;
            }
        }
        if (notifyTime <= timer && timer < notifyTime + flickTime)
        {
            if (!isDwellable)
            {
                targetOutline.enabled = false;
                targetOutline.OutlineColor = Color.yellow;
                isDwellable = true;
            }
        }

        if (notifyTime + flickTime <= timer && timer < notifyTime + flickTime + reponseTime)
        {
            if (isDwellable)
            {
                targetOutline.OutlineColor = Color.red;
                targetOutline.OutlineWidth = 5f;
                targetOutline.enabled = true;
                isDwellable = false;
                if (gazeStatus == GazeStatus.Formal)
                    dataRecorder.LogDataRow(
                        (int)Mathf.Floor((count - 1) / 11) + 1,
                        currentBall.GetComponent<Ball>().Index,
                        false,
                        3.0f
                    );
            }
        }

        if (notifyTime + flickTime + reponseTime <= timer)
        {
            //isConfirmed_twice = true;
            isConfirmed = true;
            disableTargetOutline();

            //resetData();
            //showNextTarget();
        }
    }

    public void showNextTarget()
    {
        currentTargetIndex++;
        if (count % targetNumber == 0)
        {
            updateScene();
        }
        if (currentTargetIndex == targetNumber)
        {
            SetTextBoardContext("Press Space when you are ready.");
            isPause = true;
            currentTargetIndex = 0;

            return;
        }

        count++;

        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        targetOutline = currentBall.GetComponent<Outline>();
        isDwellable = true;
        isConfirmed = false;
    }

    void initTargetIndex()
    {
        currentTargetIndex = 0;
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        targetOutline = currentBall.GetComponent<Outline>();
        isConfirmed = false;
        count++;
    }

    void resetData()
    {
        currentTime = Time.time;
        timer = 0f;
        ballsController.resetBallsOutlineColor();
        ballsController.disableBallsOutline();
    }

    public virtual void updateScene()
    {
        if (updating)
        {
            sM.updateScene();
            Debug.Log("Update the Scene");
            updating = false;
        }
    }

    void initCurosrVisibility()
    {
        if (IOManager.stateVisible == Modality.VisibilityState.visible)
        {
            // 设置为不透明白色
            Color opaqueWhite = new Color(1f, 1f, 1f, 1f); // RGBA: (1, 1, 1, 1)
            cursor.GetComponent<Renderer>().material.color = opaqueWhite;
        }
        if (IOManager.stateVisible == Modality.VisibilityState.invisible)
        {
            // 设置为全透明白色
            Color transparentWhite = new Color(1f, 1f, 1f, 0f); // RGBA: (1, 1, 1, 0)
            cursor.GetComponent<Renderer>().material.color = transparentWhite;
        }
    }

    public Ray CombineRays()
    {
        // 左眼和右眼的起点
        Vector3 leftStart = leftEyePos.transform.position;
        Vector3 rightStart = rightEyePos.transform.position;
        // 左眼和右眼的方向
        Vector3 leftDir = leftEyePos.transform.forward.normalized;
        Vector3 rightDir = rightEyePos.transform.forward.normalized;
        // 计算中间点作为新射线的起点
        Vector3 combinedStart = (leftStart + rightStart) * 0.5f;
        // 计算方向的平均值并归一化，作为新射线的方向
        Vector3 combinedDir = (leftDir + rightDir).normalized;
        // 返回新的射线
        return new Ray(combinedStart, combinedDir);
    }

    void confirmationMethods()
    {
        if (IOManager.stateCM == Modality.ModalityState.dwell)
        {
            dwell();
        }
    }

    void dwell()
    {
        combinedRay = CombineRays();
        cursor.transform.position = combinedRay.origin + combinedRay.direction * cursorDistance;
        float dwellThreshold = 0.6f;
        RaycastHit hit;
        Transform hitted; // Use combined ray for raycast detection
        if (isDwellable)
        {
            if (Physics.Raycast(combinedRay, out hit))
            {
                if (hit.transform.name.Contains("Ball"))
                {
                    ballOutline = hit.transform.gameObject.GetComponent<Outline>();
                    hitted = hit.collider.transform;
                    ballOutline.OutlineColor = Color.yellow;
                    ballOutline.enabled = true;
                    dwellTimer_dwell += Time.deltaTime;
                    float maxOutlineWidth = 5.0f;
                    ballOutline.OutlineWidth = Mathf.Lerp(
                        0,
                        maxOutlineWidth,
                        dwellTimer_dwell / dwellThreshold
                    );

                    if (dwellTimer_dwell > dwellThreshold && !isConfirmed_dwell)
                    {
                        AudioSource.PlayClipAtPoint(bingo, new Vector3(0, 0, 0)); //选择成功
                        dwellTimer_dwell = dwellThreshold;
                        isConfirmed_dwell = true;
                        ballOutline.enabled = false;
                        ballOutline.OutlineWidth = 5.0f;

                        recData.celarList();
                        confirm_dwellResult(hit.transform.name, ballOutline, dwellTimer_dwell);
                        ballOutline = null;
                    }
                }
            }
            else
            {
                dwellTimer_dwell = 0;
                if (ballOutline != null)
                {
                    ballOutline.OutlineWidth = 5.0f;
                    ballOutline.enabled = false;
                    ballOutline = null;
                }
                isConfirmed_dwell = false;
            }
        }
    }

    // dewll 确认以后判断对错
    void confirm_dwellResult(string targetName, Outline hittedOutline, float duration)
    {
        isDwellable = false;
        Debug.Log(
            targetName + "  " + currentBall.name + ": " + targetName.Equals(currentBall.name)
        );
        bool result = targetName.Equals(currentBall.name);
        if (gazeStatus == GazeStatus.Formal)
            dataRecorder.LogDataRow(
                (int)Mathf.Floor((count - 1) / 11) + 1,
                currentBall.GetComponent<Ball>().Index,
                result,
                duration
            );
        if (result)
        {
            targetOutline.OutlineColor = Color.green;
            targetOutline.OutlineWidth = 5f;
            targetOutline.enabled = true;
        }
        else
        {
            hittedOutline.OutlineColor = Color.red;
            hittedOutline.OutlineWidth = 5f;
            hittedOutline.enabled = true;
        }

        isConfirmed = true;
        Debug.Log("Confirm the result");

        //StartCoroutine(DelayedFunctionCoroutine(2.0f, targetOutline));
        Invoke("disableTargetOutline", reponseTime);
    }

    void disableTargetOutline(Outline outline)
    {
        outline.OutlineColor = Color.yellow;
        outline.enabled = false;
        //formal();
    }

    void disableTargetOutline()
    {
        SetTextBoardContext(
            gazeStatus.ToString() + " Block " + (Mathf.Floor(count / 11) + 1).ToString()
        );
        targetOutline.OutlineColor = Color.yellow;
        targetOutline.enabled = false;
        if (!updating)
            updating = true;
        if (count == 0)
            centerPoint.SetActive(false);

        showNextTarget();
        resetData();
    }

    IEnumerator DelayedFunctionCoroutine(float delay, Outline targetOutline)
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(delay);

        // 在延迟之后，调用你想要执行的函数
        disableTargetOutline(targetOutline);
    }

    Vector3 getCorrectDirection()
    {
        Vector3 correctDirection = cursor.transform.position - Vector3.zero;
        Vector3 normalizedCorrectDirection = correctDirection.normalized;
        return normalizedCorrectDirection;
    }

    void SetTextBoardContext(string context)
    {
        textBoard.text = context;
    }
}
