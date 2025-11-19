using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.ImmersiveDebugger.UserInterface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ExpList
{
    private List<float> angularDistance_O = new List<float>() { 20f, 30f, 40f }; // 直径
    private List<float> targetWidth_O = new List<float>() { 4f, 3f, 2f }; // 角度

    // private List<float> angularDistance_O = new List<float>() { 30f, 30f, 30f }; // 直径
    // private List<float> targetWidth_O = new List<float>() { 4f, 4f, 4f };// 角度
    public List<Tuple<float, float>> expSettings;
    public List<float> targetCycleHz;
    public List<float> targetCyclePhasedelay;

    // 排列组合 顺序随机 每次都要遍历 不能重复

    public List<Vector2> expSettingsVector2 = new List<Vector2>();

    public ExpList()
    {
        expSettings = new List<Tuple<float, float>>();
        initSceneSetting();
    }

    public void initSceneSetting()
    {
        Tuple<float, float>[,] combinationMatrix = CreateCombinationMatrix(
            angularDistance_O,
            targetWidth_O
        );

        List<Tuple<float, float>> shuffledList = ShuffleMatrixElements(combinationMatrix);
        shuffledList.Insert(0, new Tuple<float, float>(40, 4));
        shuffledList.Insert(0, new Tuple<float, float>(40, 4));
        Debug.Log("打乱后的实验设置: " + shuffledList.Count);

        expSettings = shuffledList;
        foreach (var (item1, item2) in shuffledList)
        {
            expSettingsVector2.Add(new Vector2(item1, item2));
        }
        // expSettingsVector2.Insert(0,new Vector2(40f,4f));
        // expSettingsVector2.Insert(1,new Vector2(40f,4f));

        // var max2 = expSettingsVector2.OrderByDescending(v => v.y).ToList()
        //   ;
        //
        // expSettingsVector2.Remove(max2[0]);
        // expSettingsVector2.Remove(max2[1]);

        // var result
    }

    public void initCycleHZ(int targetNum)
    {
        targetCycleHz = new List<float>();
        targetCyclePhasedelay = new List<float>();

        for (int i = 0; i < targetNum; i++)
        {
            targetCycleHz.Add(Mathf.Round((8 + i * 0.4f) * 10f) / 10f);
            targetCyclePhasedelay.Add(Mathf.Round((0.35f * i % 2f) * 100f) / 100f);
        }

        List<float> roundedFrequencies = targetCycleHz
            .Select(f => Mathf.Round(f * 10f) / 10f)
            .ToList();
        Debug.Log("Rounded Frequencies: " + string.Join(", ", roundedFrequencies));
        List<float> roundedphasess = targetCyclePhasedelay
            .Select(f => Mathf.Round(f * 100f) / 100f)
            .ToList();
        Debug.Log("Rounded Phases: " + string.Join(", ", roundedphasess));
    }

    private Tuple<T, U>[,] CreateCombinationMatrix<T, U>(List<T> listA, List<U> listB)
    {
        if (listA == null || listB == null || listA.Count == 0 || listB.Count == 0)
        {
            Debug.LogError("输入的列表不能为空或空。");
            return null;
        }

        int rows = listA.Count;
        int cols = listB.Count;
        Tuple<T, U>[,] matrix = new Tuple<T, U>[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = Tuple.Create(listA[i], listB[j]);
            }
        }

        return matrix;
    }

    /// <summary>
    /// 将一个二元组矩阵的所有元素随机打乱，并放入一个新的列表中。
    /// 使用了 Fisher-Yates 洗牌算法，确保随机性。
    /// </summary>
    /// <param name="matrix">要打乱的二元组矩阵。</param>
    /// <returns>一个包含所有矩阵元素且顺序随机的新列表。</returns>
    private List<Tuple<T, U>> ShuffleMatrixElements<T, U>(Tuple<T, U>[,] matrix)
    {
        if (matrix == null)
        {
            Debug.LogError("输入的矩阵不能为空。");
            return new List<Tuple<T, U>>(); // 返回一个空列表以避免错误
        }

        List<Tuple<T, U>> flatList = new List<Tuple<T, U>>();
        foreach (var element in matrix)
        {
            flatList.Add(element);
        }

        int n = flatList.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (flatList[i], flatList[j]) = (flatList[j], flatList[i]);
        }
        return flatList;
    }
}
