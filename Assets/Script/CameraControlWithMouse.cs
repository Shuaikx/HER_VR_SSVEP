using UnityEngine;

public class CameraControlWithMouse : MonoBehaviour
{
    [Header("View Angle Control Parameters")]
    [Tooltip("Mouse Sensitivity")]
    public float mouseSensitivity = 100f;

    [Tooltip("Minimum vertical angle (look up limit)")]
    public float minPitch = -85f; // Maximum upward angle (negative value)

    [Tooltip("Maximum vertical angle (look down limit)")]
    public float maxPitch = 85f; // Maximum downward angle

    [Tooltip(
        "Horizontal angle limit, speed of left/right rotation from initial forward direction, 90 means can reach 180-degree field of view"
    )]
    public float horizontalYawLimit = 90f; // Horizontal left/right rotation angle limit from initial forward direction

    private float xRotation = 0f; // Current camera's vertical rotation angle (Pitch)
    private float yRotation = 0f; // Current camera's horizontal rotation angle (Yaw)
    private float initialYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 initialEulerAngles = transform.localEulerAngles;

        // Initialize horizontal rotation (Yaw)
        initialYaw = initialEulerAngles.y; // Record initial Y-axis orientation as control reference
        yRotation = initialYaw; // Current yRotation starts accumulating from initial orientation

        // Initialize vertical rotation (Pitch)
        xRotation = initialEulerAngles.x;
        if (xRotation > 180f) // Convert Euler X range from (0,360) to (-180, 180)
        {
            xRotation -= 360f;
        }
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch); // Apply initial Pitch limits

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- Horizontal Rotation (Yaw) ---
        yRotation += mouseX; // Accumulate sensed Y-axis rotation

        // Calculate current accumulated yRotation's angle difference from initial orientation (initialYaw)
        // Mathf.DeltaAngle returns shortest difference between two angles (e.g., between 350° and 10° is 20°, not -340°)
        float angleDifferenceFromInitial = Mathf.DeltaAngle(initialYaw, yRotation);

        // Clamp angle difference within [-horizontalYawLimit, +horizontalYawLimit] range
        float clampedAngleDifference = Mathf.Clamp(
            angleDifferenceFromInitial,
            -horizontalYawLimit,
            horizontalYawLimit
        );

        // Final applied Y-axis rotation is initial orientation plus clamped angle difference
        float finalYaw = initialYaw + clampedAngleDifference;

        // --- Vertical Rotation (Pitch) ---
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        // Apply rotation
        transform.localRotation = Quaternion.Euler(xRotation, finalYaw, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
