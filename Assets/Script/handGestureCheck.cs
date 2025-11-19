using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRHand; // 方便直接使用 HandFinger 枚举

public class handGestureCheck : MonoBehaviour
{
    [Header("Hand References")]
    public OVRHand leftHand;
    public OVRHand rightHand;

    [Header("Pinch Settings")]
    [Tooltip("当 PinchStrength 大于此阈值时，视为捏合")]
    public float pinchThreshold = 0.8f;
    public bool isPinched = false;

    void Update()
    {
        // 检测右手食指和拇指的捏合
        float rightIndexPinch = rightHand.GetFingerPinchStrength(HandFinger.Index);
        float rightThumbPinch = rightHand.GetFingerPinchStrength(HandFinger.Thumb);
        isPinched = false;
        if (rightIndexPinch > pinchThreshold && rightThumbPinch > pinchThreshold)
        {
            isPinched = true;
        }

        // // 也可以根据需要检测左手：
        // float leftIndexPinch = leftHand.GetFingerPinchStrength(HandFinger.Index);
        // float leftThumbPinch = leftHand.GetFingerPinchStrength(HandFinger.Thumb);

        // if (leftIndexPinch > pinchThreshold && leftThumbPinch > pinchThreshold)
        // {
        //     Debug.Log("左手食指和拇指正在捏合！");
        // }
    }
}
