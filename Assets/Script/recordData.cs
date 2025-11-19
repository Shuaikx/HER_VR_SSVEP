using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class recordData : MonoBehaviour
{
    // calculateXY calXY;
    public GameObject centerPoint;
    public Text restTime;

    // public sceneManager scene;

    private Vector2 xy;
    private float gapTime;
    private float rWidth;
    private float distance;
    private string preName;
    private string currentName;
    private float rAmplitude;

    private int reRenterTime;
    private float ave_distance,
        std_deviation;
    private List<float> distanceList = new List<float>(); // 存储每帧的 distance 值

    public void recordreRenterTime(int time)
    {
        reRenterTime = time;
    }

    public void IOTimeAxis()
    {
        if (IOManager.stateCM.Equals("blink") || IOManager.stateCM.Equals("airTap"))
        {
            IOManager.writeResult(
                xy,
                gapTime,
                distance,
                rWidth,
                rAmplitude,
                preName,
                currentName,
                reRenterTime
            );
        }
        if (IOManager.stateCM.Equals("dwell"))
        {
            IOManager.writeResult_dwell(
                gapTime,
                ave_distance,
                std_deviation,
                rWidth,
                rAmplitude,
                preName,
                currentName,
                reRenterTime
            );
        }
        Debug.Log(ave_distance + " " + std_deviation);
        ave_distance = 0;
        std_deviation = 0;
    }

    public void recordAxis(Ball preball, Transform hitted, Vector3 dir)
    {
        xy = recordXY(preball, hitted, dir);
    }

    public void recordAxisbyFrame(Ball preball, Transform hitted, Vector3 dir)
    {
        xy = recordXY(preball, hitted, dir);
        // Debug.Log(xy.x+"  "+xy.y);
        distanceList.Add(Vector2.Distance(xy, Vector2.zero));
    }

    public void recordDewellInfo()
    {
        foreach (float dist in distanceList)
        {
            ave_distance += dist;
        }
        ave_distance /= distanceList.Count;

        // 计算方差

        foreach (float dist in distanceList)
        {
            std_deviation += Mathf.Pow(dist - ave_distance, 2);
        }
        std_deviation /= distanceList.Count;

        // 计算标准差
        float standardDeviation = Mathf.Sqrt(std_deviation);
    }

    public void celarList()
    {
        distanceList.Clear();
    }

    public void recordTime(float timeGap)
    {
        //        Debug.Log(timeGap);
        gapTime = timeGap;
    }

    public void recordWidth(float width)
    {
        // width = scene.returnVisualSize();
        rWidth = width;
    }

    public void recordAmplitude(float amplitude)
    {
        rAmplitude = amplitude;
    }

    public void recordDistance()
    {
        distance = Vector2.Distance(xy, Vector2.zero);
    }

    public void recordDirection(Ball preBall, Ball currentBall)
    {
        if (preBall == null)
            preName = "-1";
        else
            preName = preBall.transform.name;
        currentName = currentBall.transform.name;
    }

    private Vector2 recordXY(Ball preball, Transform hitted, Vector3 dir)
    {
        Vector3 start;
        Vector3 end;

        if (preball == null)
        {
            start = centerPoint.transform.position;
        }
        else
        {
            start = preball.transform.position;
        }

        end = hitted.position;

        Vector2 xy = calculateXY.single.cal(
            calculateXY.single.posToVec2(start),
            calculateXY.single.posToVec2(end),
            calculateXY.single.posToVec2(dir)
        );

        return xy;
    }

    public Vector2 Test_recordXY(Ball preball, Transform hitted, Vector3 dir)
    {
        Vector3 start;
        Vector3 end;

        if (preball == null)
        {
            start = centerPoint.transform.position;
        }
        else
        {
            start = preball.transform.position;
        }

        end = hitted.position;

        Vector2 xy = calculateXY.single.cal(
            calculateXY.single.posToVec2(start),
            calculateXY.single.posToVec2(end),
            calculateXY.single.posToVec2(dir)
        );

        return xy;
    }
}
