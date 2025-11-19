using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 引入 List 所需的命名空间

public class Ball_SSVEP : MonoBehaviour
{
    public int Index;
    public float CycleHz = 6f;       // 闪烁频率（Hz），即每秒闪烁次数
    public float PhaseDelay = 0f;    // 相位延迟（弧度）
    [SerializeField] private Color colorOrigin = Color.white;
    [SerializeField] private Color colorNotify = Color.blue;
    [SerializeField] private Color colorFlickerOn = Color.black;
    [SerializeField] private bool flickerOn = false;

    public List<Color> _flickerColorSequence = new List<Color>();
    // public Material material;
    
    private float _sequenceTimer = 0f;
    // public Color MatColor;
    // private const int ScreenRefreshRateHz = 60;
    public List<float> sineWaveData=new List<float>();
    
    private MaterialPropertyBlock _propBlock;
    private Renderer _sphereRenderer;


    private void Awake()
    {
        // 获取球体的 Renderer 组件
        _sphereRenderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
        FlickerControl.Instance.OnFlickerTurnedOn += startStimulate;
        FlickerControl.Instance.OnFlickerTurnedOff += endStimulate;
    }

    private void Start()
    {
        PrecalculateFlickerColors(); 
       
    }

    public void ChangeSphereColor(Color newColor)
    {
        // 设置颜色属性
        _propBlock.SetColor("_Color", newColor);
        
        // 应用到球体渲染器
        _sphereRenderer.SetPropertyBlock(_propBlock);
    }
    
    
    
    
    

    void Dispose()
    {
        FlickerControl.Instance.OnFlickerTurnedOn -= startStimulate;
        FlickerControl.Instance.OnFlickerTurnedOff -= endStimulate;
    }

    [SerializeField]private int stimInd = 0;
    void Update()
    {
        if (flickerOn && stimInd<240)
        {
     
            var id = sineWaveData[stimInd];

            // MatColor =    
            ChangeSphereColor(new Color( id,id,id));

            stimInd++;
            
        }
        else
        {
            stimInd = 0;
            flickerOn = false;
        }

    }

    /// <summary>
    /// 预计算一个完整闪烁周期内的所有颜色帧。
    /// </summary>
    private void PrecalculateFlickerColors()
    {
        _flickerColorSequence.Clear(); // 清空旧的序列


        int totalSamples = Mathf.FloorToInt(5 * 60);
        for (int i = 0; i < totalSamples; i++)
        {
            float time = i / 60f;
            // var value = Mathf.RoundToInt((Mathf.Sin(2f * Mathf.PI * CycleHz * time) * 0.5f + 0.5f));
            var value = ((Mathf.Sin(2f * Mathf.PI * CycleHz * time) * 0.5f + 0.5f));
// Debug.LogWarning(CycleHz);
            sineWaveData.Add(value);

        }

        List<float> sineWaveData1 = sineWaveData.Select(f => Mathf.Round(f * 10f) / 10f).ToList();
        Debug.Log("Rounded Frequencies: " + string.Join(", ", sineWaveData1));


    }

    public void startNotify()
    {
        flickerOn = false; 
        // if (MatColor != colorNotify)
        ChangeSphereColor(colorNotify);

    }

    public void endNotify()
    {
        // if (MatColor == colorNotify)
        ChangeSphereColor(colorOrigin);

    }

    public void startStimulate()
    {
        _sequenceTimer = 0f; 
        if (!flickerOn)
            flickerOn = true;
    }

    public void endStimulate()
    {
        if (flickerOn)
            flickerOn = false;
        // MatColor = colorOrigin;
        ChangeSphereColor(colorOrigin);

    }

    public void SetFlickerParameters(float newCycleHz, float newPhaseDelay)
    {
        if (CycleHz != newCycleHz || PhaseDelay != newPhaseDelay)
        {
            CycleHz = newCycleHz;
            PhaseDelay = newPhaseDelay;
            PrecalculateFlickerColors(); // 重新预计算
            _sequenceTimer = 0f; // 重置计时器
            // if (flickerOn) // 如果正在闪烁，立即更新颜色
                // MatColor = _flickerColorSequence[0];
        }
    }
}