using UnityEngine;

public class sceneUtility : MonoBehaviour
{
    //konw one long side and one included angle, calculate one short side in an isoscele triangle
    public static float outputOneSide_angle(float angularDistance, float threeIncludedAngle_angle)
    {
        float threeIncludedAngle_rad = threeIncludedAngle_angle * Mathf.PI / 180;
        return Mathf.Sqrt(
            angularDistance * angularDistance / (2 * (1 - Mathf.Cos(threeIncludedAngle_rad)))
        ); //threeIncludedAngle_rad是等腰三角形的短边，也就是人到target的实际的长度
    }

    //the first ball is right above the center, so do need to change vector3.x
    public static Vector3 outputFirstballPosition(
        Vector3 centerPoint,
        float depthDistance,
        float oneSide_angle
    )
    {
        float oneSide_rad = oneSide_angle * Mathf.PI / 180;
        Vector3 firstballPosition;
        firstballPosition.x = centerPoint.x;
        firstballPosition.y = depthDistance * Mathf.Sin(oneSide_rad);
        firstballPosition.z = depthDistance * Mathf.Cos(oneSide_rad);
        return firstballPosition;
    }

    // anticlockwise rotate, from orginal point to get the new point position after rotation
    public static Vector3 convertPositionOnce(Vector3 originPosition, float includedAngle_angle)
    {
        float rotation_rad = includedAngle_angle * Mathf.PI / 180;
        Vector3 finalPosition;
        finalPosition.x =
            originPosition.x * Mathf.Cos(rotation_rad) - originPosition.y * Mathf.Sin(rotation_rad);
        finalPosition.y =
            originPosition.x * Mathf.Sin(rotation_rad) + originPosition.y * Mathf.Cos(rotation_rad);
        finalPosition.z = originPosition.z;
        return finalPosition;
    }

    //alter target visual size to actual size 实际size
    public static float targetActualWidth(
        float targetVisualWidth_angle,
        Transform target,
        Transform eye
    )
    {
        Vector3 startPoint = eye.transform.position;
        Vector3 endPoint = target.transform.position;
        float distance = Mathf.Sqrt(
            Mathf.Pow(startPoint.x - endPoint.x, 2)
                + Mathf.Pow(startPoint.y - endPoint.y, 2)
                + Mathf.Pow(startPoint.z - endPoint.z, 2)
        );
        float halfAngle_rad = targetVisualWidth_angle / 2 * Mathf.PI / 180;
        float actualWidth = Mathf.Sin(halfAngle_rad) * distance * 2;
        return actualWidth;
    }

    // to confirm the target visual size
    public static float angularSize(Transform target)
    {
        float radius = target.lossyScale.x / 2;
        float distance = Mathf.Sqrt(
            target.position.x * target.position.x
                + target.position.y * target.position.y
                + target.position.z * target.position.z
        );
        float sinA = radius / distance;
        float A = Mathf.Asin(sinA);
        float angularSize = A / 2 / Mathf.PI * 360 * 2;
        return angularSize;
    }
}
