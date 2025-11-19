using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowFPS : MonoBehaviour
{
    private float m_LastUpdateShowTime = 0f;  //上一次更新帧率的时间;  
    private float m_UpdateShowDeltaTime = 0.01f;//更新帧率的时间间隔;  
    private int m_FrameUpdate = 0;//帧数;  
    private float m_FPS = 0;
    void Awake()
    {
        Application.targetFrameRate = -1;
    }

    // Use this for initialization  
    void Start()
    {
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
    }

	[Tooltip("Number of frames to average together in frame rate statistics (1 corresponds to no averaging)")]
	[Range(1,999)]
	public int SampleWindow = 50;

	float lastRealtime = 0.0f;
	float lastTime = 0.0f;

	List<float> realtimeDeltas = new List<float>();
	int realtimeDeltaPos = 0;
	float realtimeDeltasSum = 0.0f;

	List<float> timeDeltas = new List<float>();
	int timeDeltaPos = 0;
	float timeDeltasSum = 0.0f;

	static float GetAverage(ref List<float> values, ref int valuePos, ref float sum, float newValue, int historySize)
	{
		if (historySize <= 0)
			historySize = 1;

		if (values.Count < historySize)
		{
			while (valuePos != 0)
			{
				values.Add(values[0]);
				values.RemoveAt(0);
				--valuePos;
			}
			values.Add(newValue);
			sum += newValue;
			return -1.0f;
		}

		while (values.Count > historySize)
		{
			sum -= values[valuePos];
			values.RemoveAt(valuePos);
			if (valuePos >= historySize)
				valuePos = 0;
		}

		sum = sum - values[valuePos] + newValue;
		values[valuePos] = newValue;
		valuePos = (valuePos + 1) % historySize;
		return sum / historySize;
	}


    void OnGUI()
    {
		// Fontsize setting JC add for fonts size 20210208
		GUIStyle btnStyle = new GUIStyle("Label");
		btnStyle.fontSize = 20;

		float realtime = Time.realtimeSinceStartup;
		float time = Time.time;

		if (lastRealtime > 0.0f && time > 0.0f)
		{
			float deltaRealtime = GetAverage(
				ref realtimeDeltas,
				ref realtimeDeltaPos,
				ref realtimeDeltasSum,
				realtime - lastRealtime,
				SampleWindow
			);

			float deltaTime = GetAverage(
				ref timeDeltas,
				ref timeDeltaPos,
				ref timeDeltasSum,
				time - lastTime,
				SampleWindow
			);

			if (deltaRealtime >= 0.0f && deltaTime >= 0.0f)
				//Text_showFPS.text =
					//"Render FPS: " + (1.0f/deltaRealtime).ToString("0.0") + " (" + (deltaRealtime*1000).ToString("0.0ms") + ")\n" +
					//"  Game FPS: " + (1.0f/deltaTime).ToString("0.0") + " (" + (deltaTime*1000).ToString("0.0ms") + ")";
				//GUI.Label(new Rect(0, 0, 200, 200), "FPS: " + m_FPS.ToString("00"));
				GUI.Label(new Rect(0, 0, 200, 200), "Render FPS: " + (1.0f/deltaRealtime/2.0f).ToString("0.00") + " (" + (deltaRealtime*1000*2).ToString("0.00ms") + ")\n" +
				"Game FPS: " + (1.0f/deltaTime/2.0f).ToString("0.00") + " (" + (deltaTime*1000*2).ToString("0.00ms") + ")",btnStyle);
		}

		lastRealtime = realtime;
		lastTime = time;		
    }
}  

