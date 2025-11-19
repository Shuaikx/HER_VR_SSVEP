using UnityEngine;

public class CameraControlWithMouse : MonoBehaviour
{
    [Header("视角控制参数")]
    [Tooltip("鼠标灵敏度")]
    public float mouseSensitivity = 100f;

    [Tooltip("垂直视角的最小角度（向上看多少）")]
    public float minPitch = -85f; // 向上看的最大角度（通常是负值）

    [Tooltip("垂直视角的最小角度（向下看多少）")]
    public float maxPitch = 85f;  // 向下看的最大角度

    [Tooltip("水平视角限制（从初始方向左右各能转多少度，90表示总共180度视野）")]
    public float horizontalYawLimit = 90f; // 水平方向从中心点左右各能旋转的角度

    private float xRotation = 0f; // 当前摄像机的垂直旋转角度 (Pitch)
    private float yRotation = 0f; // 当前摄像机的水平旋转角度 (Yaw)
    private float initialYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 initialEulerAngles = transform.localEulerAngles;

        // 初始化水平旋转 (Yaw)
        initialYaw = initialEulerAngles.y; // 记录初始的Y轴朝向作为限制的中心
        yRotation = initialYaw;            // 当前的yRotation从初始朝向开始累积

        // 初始化垂直旋转 (Pitch)
        xRotation = initialEulerAngles.x;
        if (xRotation > 180f) // 将欧拉角X的范围从 (0,360) 转换到 (-180, 180)
        {
            xRotation -= 360f;
        }
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch); // 应用初始的Pitch限制

        // 应用初始旋转，确保与计算值一致
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- 水平旋转 (Yaw) ---
        yRotation += mouseX; // 累积总的Y轴旋转

        // 计算当前累积的yRotation相对于初始朝向(initialYaw)的角度差
        // Mathf.DeltaAngle 会返回两个角度之间的最短差异（例如350度和10度之间是20度，而不是-340度）
        float angleDifferenceFromInitial = Mathf.DeltaAngle(initialYaw, yRotation);

        // 将这个角度差限制在 [-horizontalYawLimit, +horizontalYawLimit] 范围内
        float clampedAngleDifference = Mathf.Clamp(angleDifferenceFromInitial, -horizontalYawLimit, horizontalYawLimit);

        // 最终应用的Y轴旋转是初始朝向加上被限制了的角度差
        float finalYaw = initialYaw + clampedAngleDifference;

        // --- 垂直旋转 (Pitch) ---
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        // 应用旋转
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