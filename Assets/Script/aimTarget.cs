using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Diagnostics.Tracing;

public class aimTarget : MonoBehaviour
{
    [SerializeField] private int trialCount = 10;
    public IOManager ioM;
    public OVRFaceExpressions faceExpressions;
    public GameObject cursor;
    public GameObject leftEyePos;
    public GameObject rightEyePos;
    public GameObject cam;
    public GameObject centerPoint;
    public GameObject Text;
    public AudioClip bingo;
    public handGestureCheck handGesture;

    private Text restTime;
    private Ball ball;
    private Ball preBall;
    private GameObject currentBall;
    public recordData recData;
    public sceneManager sM;

    [Header("小球Controller")]
    public GazeBallsController BallsController;

    private float currentTime;
    private Ray combinedRay;
    private bool updating = true;
    bool startGame = false;
    int targetNumber;
    int currentTargetIndex;
    int count = 0;
    private Vector3 frozenCursorPosition_blink;
    private float freezeTimer_blink = 0f;
    private bool isBlinking =false;
    private bool isConfirmed_blink = false;
    private bool isConfirmed_dwell = false;
    private float dwellTimer_dwell = 0f;
    private bool isConfirmed_airTap = false;
    private bool isConfirmed_once = false;
    public static bool inPractice = true;
    //private int reRenterTime = 0;
    private bool hasHitTarget = false;
    private bool middleHasHitTarget = false;
    private Outline ballOutline = null;

    private OVRInput.Button buttonA = OVRInput.Button.One;  // 默认是 A 按钮
    private OVRInput.Button buttonB = OVRInput.Button.Two;  // 默认是 B 按钮

    // Use this for initialization
    void Start()
    {
        targetNumber = sceneManager.Instance.targetNumber;
        initTargetIndex();
        restTime = Text.GetComponent<Text>();
        restTime.text = "Practice Phase";
        initCurosrVisibility(); //决定cursor的显示
    }

    // Update is called once per frame
    void Update()
    {
        confirmationMethods();
        baselineFeedback(); //在新实验里，不需要outline让用户察觉自己已经看到了这个物体

        if (inPractice)
        {
            if (isConfirmed_once || Input.GetKeyDown("s") || OVRInput.GetDown(buttonB))
            {
                ball.changeColorToWhite();
                showNextTarget();
            }
            if (Input.GetKeyDown("z") || OVRInput.GetDown(buttonA))
            {
                ball.changeColorToWhite();
                initTargetIndex();
                inPractice = false;
                restTime.text = "Clabirate Phase";
            }
        }

        if(!inPractice)
            clabiratedAndStart();

        if ((isConfirmed_once || Input.GetKeyDown("s") || OVRInput.GetDown(buttonB)) && !inPractice)
        {
            if (startGame)
            {
                if (!updating)
                    updating = true;
                if(count == 0)
                {
                    centerPoint.SetActive(false);
                }
                if(IOManager.stateCM == Modality.ModalityState.airTap)// 如果是blink或者airTap就正常记录
                {
                    Transform hitted = rayShoot();
                    recData.recordAxis(preBall, hitted, getCorrectDirection());
                    recData.recordDistance();
                }
                //就只有这个值决定了cam的位置导致的xy偏移
                recData.recordTime(Time.time - currentTime);
                recData.recordWidth(sM.returnVisualSize());
                recData.recordAmplitude(sM.returnAngularDistance());
                recData.recordDirection(preBall, ball);
                //recData.recordreRenterTime(reRenterTime);
                recData.IOTimeAxis();
                reSetData();

                ball.changeColorToWhite();
                showNextTarget();
                count++;
                if(count == trialCount)
                {
                    IOManager.finishExperiment();
                    SceneManager.LoadScene("over");
                }
                restTime.text = (Mathf.Floor((99 - count)  / 11)).ToString();
            }
        }

        updateScene();
    }

    void clabiratedAndStart()
    {
        if (!startGame)
        {
            if (Input.GetKeyDown("z") || OVRInput.GetDown(buttonA))
            {
                sM.updateOriginSence();
                preBall = null;
                startGame = true;
                currentTime = Time.time;
                // preShootDirection = cam.transform.forward;
                if (IOManager.stateCM== Modality.ModalityState.blink || IOManager.stateCM == Modality.ModalityState.airTap)
                {
                    IOManager.createUser();
                }
                if (IOManager.stateCM == Modality.ModalityState.dwell)
                {
                   IOManager.createUser_dwell();
                }
            }
        }
    }

    void showCurrentTarget()
    {
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        ball = currentBall.GetComponent<Ball>();
        ball.changeColorToBlue();
        addTargetIndex();
    }

    public void showNextTarget()
    {
        int currentTargetIndex = 0;
        string targetName = "Ball" + currentTargetIndex;
        currentBall = GameObject.Find(targetName);
        ball = currentBall.GetComponent<Ball>();
        ball.changeColorToBlue();
        addTargetIndex();
    }

    void initTargetIndex()
    {
        currentTargetIndex = 0;
        showCurrentTarget();
    }

    void addTargetIndex()
    {
        if (currentTargetIndex == targetNumber - 1)
        {
            currentTargetIndex = 0;
        }
        else currentTargetIndex++;
    }

    void retrievePreBall()
    {
        if (preBall != null)
        {
            preBall.changeColorToWhite();
            preBall.alterToPrimitiveSize();
        }
    }

    void reSetData()
    {
        currentTime = Time.time;
        // preDirection = cam.transform.forward;
        preBall = ball;
        // preShootDirection = cam.transform.forward;
    }

    void updateScene()
    {
        if (updating)
        {
            if (count % targetNumber == 0 && count != 0)
            {
                sM.updateScene();
                updating = false;
            }
        }
    }

    void baselineFeedback() // 控制outline
    {
        combinedRay = CombineRays();
        RaycastHit hit;
        if (Physics.Raycast(combinedRay, out hit))
        {
            // 如果击中了名称包含“Ball”的物体
            if (hit.collider.name.Equals(currentBall.name))
            {
                currentBall.GetComponent<Outline>().enabled = true;
            }
        }
        else
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                // 检查根对象的名字是否包含 "ball"
                if (obj.name.Contains("Ball"))
                {
                    // 获取 Outline 组件并禁用
                    Outline outline = obj.GetComponent<Outline>();
                    if (outline != null)
                       outline.enabled = false;
                }
            }
        }
    }


    Transform rayShoot()
    {
        // 获取合并后的射线
        combinedRay = CombineRays();
        RaycastHit hit;
        Transform hitted;// 使用合并射线进行射线检测
        middleHasHitTarget = hasHitTarget;
        if (Physics.Raycast(combinedRay, out hit))
        {
            if (hit.transform.name.Equals(currentBall.name))
            {
                hitted = hit.collider.transform;

                hasHitTarget = true;
                //if(!middleHasHitTarget & hasHitTarget & !inPractice)
                //{
                //    reRenterTime++;
                //}
                //Debug.Log($"Target Object Hit! ReRenterTime: {reRenterTime}");
            }
            else
            {
                hitted = ball.transform;
                hasHitTarget = false;
            }
        }
        else
        {
            hitted = ball.transform;
            hasHitTarget = false;
        }
        cursor.transform.position = combinedRay.origin + combinedRay.direction * 97.0f;// 更新光标位置为射线方向的远端位置
        return hitted;
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

    void initCurosrVisibility()
    {
        if (IOManager.stateVisible == Modality.VisibilityState.visible)
        {
            // 设置为不透明白色
            Color opaqueWhite = new Color(1f, 1f, 1f, 1f); // RGBA: (1, 1, 1, 1)
            cursor.GetComponent<Renderer>().material.color = opaqueWhite;
        }
        if (IOManager.stateVisible == Modality.VisibilityState.invisible){
            // 设置为全透明白色
            Color transparentWhite = new Color(1f, 1f, 1f, 0f); // RGBA: (1, 1, 1, 0)
            cursor.GetComponent<Renderer>().material.color = transparentWhite;
        }
    }
     
    void confirmationMethods()
    {
        isConfirmed_once = false;
        if (IOManager.stateCM == Modality.ModalityState.blink)
        {
            blink();
        }
        if (IOManager.stateCM == Modality.ModalityState.dwell)
        {
            dwell();
        }
        if (IOManager.stateCM == Modality.ModalityState.airTap)
        {
            airTap();
        }
    }

    void airTap()
    {
        rayShoot();
        if(handGesture.isPinched && !isConfirmed_airTap)
        {
            isConfirmed_airTap = true;
            AudioSource.PlayClipAtPoint(bingo, new Vector3(0, 0, 0));
            isConfirmed_once = true;
        }

        if(!handGesture.isPinched)
        {
            isConfirmed_airTap = false;
        }
    }

    void dwell()
    {
        combinedRay = CombineRays();
        middleHasHitTarget = hasHitTarget;
        cursor.transform.position = combinedRay.origin + combinedRay.direction * 97.0f;
        float dwellThreshold = 0.6f;
        RaycastHit hit;
        Transform hitted;// 使用合并射线进行射线检测
        //Outline outline = currentBall.GetComponent<Outline>();
        //outline.enabled = true;
        
        if (Physics.Raycast(combinedRay, out hit))
        {
            if (hit.transform.name.Contains("Ball"))
            {
                ballOutline = hit.transform.gameObject.GetComponent<Outline>();
                hitted = hit.collider.transform;

                hasHitTarget = true;
                recData.recordAxisbyFrame(preBall, hitted, getCorrectDirection());

                ballOutline.enabled = true;
                dwellTimer_dwell += Time.deltaTime;
                float maxOutlineWidth = 4.0f;
                ballOutline.OutlineWidth = Mathf.Lerp(0, maxOutlineWidth, dwellTimer_dwell / dwellThreshold);

                if (dwellTimer_dwell > dwellThreshold & !isConfirmed_dwell)
                {
                    recData.recordDewellInfo();
                    AudioSource.PlayClipAtPoint(bingo, new Vector3(0, 0, 0));//选择成功
                    dwellTimer_dwell = dwellThreshold;
                    isConfirmed_dwell = true;
                    ballOutline.OutlineWidth = 0;
                    isConfirmed_once = true;
                    recData.celarList();
                    confirm_dwellResult(hit.transform.name);
                }

                
                
                
            }
            else
            {
                hasHitTarget = false;
            }
        }
        else
        {
            hasHitTarget = false;
            recData.celarList();
            dwellTimer_dwell = 0;
            if (ballOutline != null)
            {
                ballOutline.OutlineWidth = 0;
                ballOutline.enabled = false;
                ballOutline = null;
            }
            isConfirmed_dwell = false;
            isConfirmed_once = false;
        }
    }

    // dewll 确认以后判断对错
    void confirm_dwellResult(string targetName)
    {
        if (targetName.Equals(currentBall.name))
        {

        }
    }

    void dwell_origin()
    {
        combinedRay = CombineRays();
        middleHasHitTarget = hasHitTarget;
        cursor.transform.position = combinedRay.origin + combinedRay.direction * 97.0f;
        float dwellThreshold = 0.6f;
        RaycastHit hit;
        Transform hitted;// 使用合并射线进行射线检测
        Outline outline = currentBall.GetComponent<Outline>();
        outline.enabled = true;
        if (Physics.Raycast(combinedRay, out hit))
        {
            if (hit.transform.name.Equals(currentBall.name))
            {
                hitted = hit.collider.transform;

                hasHitTarget = true;
                //if(!middleHasHitTarget & hasHitTarget & !inPractice)
                //{
                //    reRenterTime++;
                //}
                //Debug.Log($"Target Object Hit! ReRenterTime: {reRenterTime}");

                recData.recordAxisbyFrame(preBall, hitted, getCorrectDirection());

                outline.enabled = true;
                dwellTimer_dwell += Time.deltaTime;
                float maxOutlineWidth = 3.0f;
                outline.OutlineWidth = Mathf.Lerp(0, maxOutlineWidth, dwellTimer_dwell / dwellThreshold);

                if (dwellTimer_dwell > dwellThreshold & !isConfirmed_dwell)
                {
                    recData.recordDewellInfo();
                    AudioSource.PlayClipAtPoint(bingo, new Vector3(0, 0, 0));//选择成功
                    dwellTimer_dwell = dwellThreshold;
                    isConfirmed_dwell = true;
                    outline.OutlineWidth = 0;
                    isConfirmed_once = true;
                    recData.celarList();
                }
            }
            else
            {
                hasHitTarget = false;
            }
        }
        else
        {
            hasHitTarget = false;
            recData.celarList();
            dwellTimer_dwell = 0;
            outline.OutlineWidth = 0;
            isConfirmed_dwell = false;
            isConfirmed_once = false;
        }
    }

    void blink()
    {
        if(!isBlinking)//不在眨眼过程中的时候就一直探测
        {
            rayShoot();
        }
        float blinkThreshold = 0.03f;
        float leftBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float rightBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

        bool isLeftEyeBlinking = leftBlink > blinkThreshold;
        bool isRightEyeBlinking = rightBlink > blinkThreshold;
        bool isBlinkingDetected  = isLeftEyeBlinking && isRightEyeBlinking;
        // restTime.text = isBlinkingDetected.ToString();

        if (isBlinkingDetected)
        {
            if (!isBlinking)
            {
                // 开始眨眼，冻结 cursor 位置 保存住，如果成功的话就记录
                frozenCursorPosition_blink = cursor.transform.position;
                isBlinking = true;
                freezeTimer_blink = 0f;
            }
        }
        else//没有眨眼或者中途取消眨眼的话直接变成false
        {
            isBlinking = false;
            freezeTimer_blink = 0f;
            isConfirmed_blink = false;//重置状态允许选择
        }

        if (isBlinking & !isConfirmed_blink)//如果开始眨眼的话一定要满足多少时间才可以
        {
            freezeTimer_blink += Time.deltaTime;
            if (freezeTimer_blink >= 0.15 || !isBlinkingDetected)
            {
                // 结束眨眼，解除冻结
                AudioSource.PlayClipAtPoint(bingo, new Vector3(0, 0, 0));//选择成功
                isBlinking = false;
                freezeTimer_blink = 0f;
                Transform hitted = rayShoot();
                recData.recordAxis(preBall, hitted, getCorrectDirection(frozenCursorPosition_blink));
                recData.recordDistance();
                isConfirmed_blink = true;//表示选择完成，禁止连续选择
                isConfirmed_once = true;
            }
        }
    }

    Vector3 getCorrectDirection()
    {
        Vector3 correctDirection = cursor.transform.position - Vector3.zero;
        Vector3 normalizedCorrectDirection = correctDirection.normalized;
        return normalizedCorrectDirection;
    }

    Vector3 getCorrectDirection(Vector3 frozenCursorPosition_blink)
    {
        Vector3 correctDirection = frozenCursorPosition_blink - Vector3.zero;
        Vector3 normalizedCorrectDirection = correctDirection.normalized;
        return normalizedCorrectDirection;
    }
}
