using UnityEngine;
using UnityEngine.UI;

public class T2_Occlusion : MonoBehaviour
{
    public GameObject Ball1;
    public GameObject Ball2;

    [Tooltip("Desired occlusion of Ball2 by Ball1 (0.0 to 1.0)")]
    [Range(0f, 1f)]
    public float TargetOcclusion = 0.5f;

    private Camera mainCamera;
    private const float BallPhysicalRadius = 0.5f; // Given diameter 1

    void Start()
    {
        mainCamera = Camera.main;
        if (Ball1 == null || Ball2 == null)
        {
            Debug.LogError("Ball1 or Ball2 GameObjects are not assigned.");
            enabled = false; // Disable this script
            return;
        }
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not found. Ensure your camera is tagged 'MainCamera'.");
            enabled = false; // Disable this script
            return;
        }

        if (Ball1.transform.localScale != Vector3.one || Ball2.transform.localScale != Vector3.one)
        {
            Debug.LogWarning("For accurate occlusion, Ball1 and Ball2 should have a scale of (1,1,1) as their size is assumed to be 1 unit diameter.");
        }

        SetOcclusion();
    }

    public void SetOcclusion()
    {
        // --- 1. Get initial positions and distances from camera ---
        Vector3 ball1InitialWorldPos = Ball1.transform.position;
        Vector3 ball2WorldPos = Ball2.transform.position;
        Vector3 camPos = mainCamera.transform.position;
        Vector3 camForward = mainCamera.transform.forward;

        float distCamToBall1Plane = Vector3.Dot(ball1InitialWorldPos - camPos, camForward);
        float distCamToBall2Plane = Vector3.Dot(ball2WorldPos - camPos, camForward);

        if (distCamToBall1Plane <= 0 || distCamToBall2Plane <= 0)
        {
            Debug.LogError("Both balls must be in front of the camera.");
            return;
        }

        if (distCamToBall1Plane >= distCamToBall2Plane && TargetOcclusion > 0.01f)
        {
            Debug.LogWarning("Ball1 is not closer to the camera than Ball2. Meaningful occlusion might not be achievable as configured.");
        }

        // --- 2. Calculate apparent radii ---
        float r2_apparent = BallPhysicalRadius;
        float r1_apparent = BallPhysicalRadius * (distCamToBall2Plane / distCamToBall1Plane);

        // --- 3. Find target separation 'd' for the projected centers ---
        float targetSeparation_d = FindSeparationForOcclusion(r1_apparent, r2_apparent, TargetOcclusion);

        // --- 4. Position Ball1 ---
        // MODIFICATION START: Get a random direction on the camera's XY plane (screen plane).
        Vector2 randomDirection2D = Random.insideUnitCircle;
        if (randomDirection2D == Vector2.zero) // Extremely rare, but handle it
        {
            randomDirection2D = Vector2.right; // Default to right if (0,0) is generated
        }
        randomDirection2D.Normalize(); // Ensure it's a unit vector

        // Convert the 2D screen direction to a 3D world space direction
        // relative to the camera's orientation.
        Vector3 offsetDirectionWorld = (mainCamera.transform.right * randomDirection2D.x +
                                        mainCamera.transform.up * randomDirection2D.y).normalized;
        // MODIFICATION END

        // This is the target position for Ball1's *projection* onto Ball2's plane.
        Vector3 ball1_projected_target_on_b2_plane = ball2WorldPos + offsetDirectionWorld * targetSeparation_d;

        Vector3 directionToProjectedTarget = (ball1_projected_target_on_b2_plane - camPos).normalized;

        float cosAngle = Vector3.Dot(directionToProjectedTarget, camForward);
        if (cosAngle <= 0.0001f)
        {
            Debug.LogError("Cannot position Ball1: target projection is too far sideways from camera's perspective.");
            return;
        }
        float distanceAlongRay = distCamToBall1Plane / cosAngle;
        Vector3 newBall1WorldPos = camPos + directionToProjectedTarget * distanceAlongRay;

        Ball1.transform.position = newBall1WorldPos;
    }

    private float FindSeparationForOcclusion(float r1_occluder, float r2_occluded, float targetOcclusionFraction)
    {
        if (targetOcclusionFraction >= 0.999f)
        {
            return 0f;
        }
        if (targetOcclusionFraction <= 0.001f)
        {
            return r1_occluder + r2_occluded;
        }

        float targetArea = targetOcclusionFraction * Mathf.PI * r2_occluded * r2_occluded;
        float d_low = 0f;
        float d_high = r1_occluder + r2_occluded;
        int iterations = 30;

        for (int i = 0; i < iterations; i++)
        {
            float d_mid = (d_low + d_high) / 2f;
            float currentArea = CalculateIntersectionArea(r1_occluder, r2_occluded, d_mid);

            if (currentArea > targetArea)
            {
                d_low = d_mid;
            }
            else
            {
                d_high = d_mid;
            }
        }
        return (d_low + d_high) / 2f;
    }

    public static float CalculateIntersectionArea(float r1, float r2, float d)
    {
        if (d <= 0.00001f) d = 0.00001f;

        if (d >= r1 + r2)
        {
            return 0f;
        }

        if (d <= Mathf.Abs(r1 - r2))
        {
            float smallerRadius = Mathf.Min(r1, r2);
            return Mathf.PI * smallerRadius * smallerRadius;
        }

        float d2 = d * d;
        float r1_2 = r1 * r1;
        float r2_2 = r2 * r2;

        float acos_arg1_num = d2 + r1_2 - r2_2;
        float acos_arg1_den = 2 * d * r1;
        float acos_arg1 = (acos_arg1_den == 0) ? 0 : Mathf.Clamp(acos_arg1_num / acos_arg1_den, -1f, 1f);

        float acos_arg2_num = d2 + r2_2 - r1_2;
        float acos_arg2_den = 2 * d * r2;
        float acos_arg2 = (acos_arg2_den == 0) ? 0 : Mathf.Clamp(acos_arg2_num / acos_arg2_den, -1f, 1f);

        float term1 = r1_2 * Mathf.Acos(acos_arg1);
        float term2 = r2_2 * Mathf.Acos(acos_arg2);
        float triangleAreaTermVal = (-d + r1 + r2) * (d + r1 - r2) * (d - r1 + r2) * (d + r1 + r2);
        float term3 = 0.5f * Mathf.Sqrt(Mathf.Max(0f, triangleAreaTermVal));

        return term1 + term2 - term3;
    }
}