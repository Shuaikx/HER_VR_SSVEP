using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Threading;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Unity.VisualScripting;

/*
 * center point and eye posint should be calibrated in the future
 * custom target number, target visual width, angular distance for the exp
 * Angular Distance measured by angle
 * Depth Distance measured by z
 * First ball is right above the center
 * OneSide_angle affected by angularDistance
 * First ball position affected by centerPoint position, depthDistance, oneSide_angle
 * Fitts circle ball sequence affected by centerPoint position, angularDistance, depthDistance (first ball position) And target number.
 */

public class sceneManager : MonoBehaviour
{

    public static sceneManager Instance;
    public GameObject firstBall;
    public GameObject eye;
    public GameObject center;
    public GameObject Balls;
    public bool IsSSVEP = false;
[Header("Target Number")]
    //set the following three factors
   [SerializeField]public int targetNumber =1; // todo:
    
    public static int expCount = 0;
    //public static int expCount2 = 0;
    //the distance from eyes to the certer
    float depthDistance = 100; // Depth

    Vector3 centerPoint; // Circle center position, needs to reset to (0,0,depthDistance) every time scene updates
    Vector3 firstballPosition;

    //miminum included angle between two sides (side is from one target to center point)
    [Header("Current Scene Settings")]
    public float targetVisualWidth_angle = 3;
    public float angularDistance = 20;

    float includedAngle_angle;
    float threeIncludedAngle_angle;
    float oneSide_angle;
    ExpList expList;
    private bool isPratice = true;
    public List<Vector2> expSettingsVector2;
    private void Awake()
    {
        Instance = this;
        expList = new ExpList();
        expList.initCycleHZ(targetNumber);
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;
        expSettingsVector2 = expList.expSettingsVector2;
        Debug.LogWarning($"@@@@{angularDistance}---{includedAngle_angle}");
        includedAngle_angle = (float)360 / targetNumber;
        threeIncludedAngle_angle = 5 * includedAngle_angle;
        centerPoint = new Vector3(0, 0, depthDistance);
        if (isPratice)// Initial size
        {
            angularDistance = 30f;
            targetVisualWidth_angle = 4f;
        }
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);
        Debug.LogWarning($"oneSide_angle {oneSide_angle}");
        produceFittsCircle(targetNumber, firstBall, firstballPosition); // Generate balls
    }

    
    public void setIsPratice(bool state)
    {
        isPratice = state;
    }

    // Initial scene setup
    public void updateOriginSence()
    {
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;
        if (isPratice)
        {
            angularDistance = 30f;
            targetVisualWidth_angle = 4f;
        }
        centerPoint = new Vector3(0, 0, depthDistance) + eye.transform.position;
        center.transform.position = centerPoint;
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);

        changeBallPosition(targetNumber, firstBall);
    }
    // Scene update - size and spacing change
    public void updateScene()
    {
        expCount++;
        if(expCount >= 9+2)
        {
            expCount = 0;
            return;
        }
        //Debug.Log("expCount2:" + expCount2);
        Debug.Log("Scene Index: " + (expCount+1));
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;
        Debug.Log("Target Width: " + targetVisualWidth_angle + ", Angular Distance: " + angularDistance);
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);
        changeBallPosition(targetNumber, firstBall);
    }
    
    
    // Create 11 balls
    private void produceFittsCircle(int targetNumber, GameObject originBall, Vector3 startBallPosition)
    {
        firstballPosition = sceneUtility.outputFirstballPosition(centerPoint, depthDistance, oneSide_angle);
     Debug.LogWarning(firstballPosition);
        originBall.transform.position = firstballPosition;
        float targetActualWidth = sceneUtility.targetActualWidth(targetVisualWidth_angle, originBall.transform, eye.transform);
        originBall.transform.localScale *= targetActualWidth;
        originBall.name = "Ball0";
        Ball_SSVEP SSVEP_O = originBall.GetComponent<Ball_SSVEP>();
        Ball ball_O = originBall.GetComponent<Ball>();
        if (ball_O)
        {
            ball_O.Index = 1;
        }
        if (SSVEP_O)
        {
            SSVEP_O.Index = 1;
            SSVEP_O.CycleHz = expList.targetCycleHz[0];
            SSVEP_O.PhaseDelay = expList.targetCyclePhasedelay[0];
        }

        Vector3 preBallPosition = firstballPosition;
        for (int i = 0; i < targetNumber - 1; i++) {
            Vector3 convertOncePosition = sceneUtility.convertPositionOnce(preBallPosition, threeIncludedAngle_angle);
            GameObject clone = GameObject.Instantiate(originBall, convertOncePosition, Quaternion.identity);
            clone.transform.parent = Balls.transform;
            Ball_SSVEP SSVEP = clone.GetComponent<Ball_SSVEP>();
            // Ball ball = clone.GetComponent<Ball>();
            // Debug.LogWarning(ball); // 没有挂在脚本 
            
            int index = i + 1;
            if (SSVEP) {
                SSVEP.Index = index+1;
                SSVEP.CycleHz = expList.targetCycleHz[index];
                SSVEP.PhaseDelay = expList.targetCyclePhasedelay[index];
            }
            // if (ball)
            // {
            //     ball.Index = index + 1;
            // }
            clone.name = "Ball" + (index).ToString();
            clone.tag = "ball";
            preBallPosition = convertOncePosition;
        }
    }

    private void changeBallPosition(int targetNumber, GameObject originBall)
    {
        // Record previous ball position
        firstballPosition = sceneUtility.outputFirstballPosition(centerPoint, depthDistance, oneSide_angle);
        // Move origin ball to that position
        originBall.transform.position = firstballPosition;
        // Change target ball to preset width
        float targetActualWidth = sceneUtility.targetActualWidth(targetVisualWidth_angle, originBall.transform, eye.transform);
        originBall.transform.localScale = new Vector3(1,1,1);
        originBall.transform.localScale *= targetActualWidth; 

        var preBallPosition = firstballPosition;
        for (int i = 0; i < targetNumber - 1; i++)
        {
            Vector3 convertOncePosition = sceneUtility.convertPositionOnce(preBallPosition, threeIncludedAngle_angle);
            GameObject gb = GameObject.Find("Ball" + (i + 1).ToString());
            gb.transform.position = convertOncePosition;
            gb.transform.localScale = new Vector3(1, 1, 1);
            gb.transform.localScale *= targetActualWidth;

            preBallPosition = convertOncePosition;
        }
    }

    public float returnVisualSize()
    {
        return targetVisualWidth_angle;
    }

    public float returnAngularDistance()
    {
        return angularDistance;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public static void QuitApplication()
    {
        Debug.Log("Attempting to quit program...");

        // Check if running in Unity Editor
#if UNITY_EDITOR
        // If in Editor, stop Play mode
        EditorApplication.isPlaying = false;
#else
        // If in packaged program, call Application.Quit()
        Application.Quit();
#endif

        // Note: Application.Quit() in packaged program doesn't exit immediately,
        // usually happens after current frame ends or later.
    }
}