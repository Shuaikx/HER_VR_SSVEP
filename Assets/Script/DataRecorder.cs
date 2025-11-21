using System; // 用于获取当前时间
using System.IO; // 用于文件操作
using System.Text; // 用于 StringBuilder
using UnityEngine;

public enum ExpType
{
    Gaze_Task1,
    SSVEP_Task1,
    Gaze_Task2,
    SSVEP_Task2,
}

public class DataRecorder : MonoBehaviour
{
    [Header("User Settings")]
    public int UserID = 0;
    public string UserName = "Test";
    public ExpType expType;

    [Header("Scene Settings")]
    public sceneManager sceneManager;
    public T2_SceneManager sceneManager_t2;

    [Header("File Settings")]
    [Tooltip("CSV file save relative path (relative to project root or packaged Data folder)")]
    private string relativeFolderPath = "ExperimentData"; // CSV file save relative folder path

    [Tooltip("CSV filename prefix")]
    private string fileNamePrefix = "Data_"; // CSV filename prefix

    private string currentFilePath = ""; // Full path of current file being written
    private StringBuilder csvBuilder = new StringBuilder(); // For efficient CSV row construction

    // Data properties (these will be CSV column headers, also required when recording data)
    // You can adjust the order and names of these properties as needed
    private readonly string[] columnHeaders = new string[]
    {
        "UserID",
        "Timestamp",
        "ExpType",
        "BlockIndex",
        "TargetIndex",
        "PhaseRadius",
        "TargetVisualAngular",
        "Result", // bool 类型
        "SelectionTime",
    };

    /// <summary>
    /// Create a new CSV file and write column headers.
    /// If file already exists, create a new unique filename with timestamp.
    /// </summary>
    /// <param name="userID">User ID, will be used as part of the filename.</param>
    public void CreateNewLogFile()
    {
        // 清空 StringBuilder 以备新文件使用
        csvBuilder.Clear();

        // Ensure folder exists
        string folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath); // Go back to project root then enter specified folder
        if (Application.isEditor) // In editor, dataPath is Assets folder
        {
            folderPath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                relativeFolderPath
            );
        }
        else // In packaged application, dataPath is <AppName>_Data folder
        {
            folderPath = Path.Combine(Application.dataPath, relativeFolderPath);
            // Or, if you want it next to the executable:
            // folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath);
            // Directory.GetParent(Application.dataPath).FullName might not work or point to wrong location in packaged build,
            // Application.persistentDataPath is a more reliable cross-platform writable path option.
            // For simplicity, we use relative to Data folder here.
            // For packaged version, Application.persistentDataPath is recommended
            // folderPath = Path.Combine(Application.persistentDataPath, relativeFolderPath);
        }

        if (!Directory.Exists(folderPath))
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"Folder created successfully: {folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create folder: {folderPath}\nError: {e.Message}");
                return;
            }
        }

        // Generate filename with user ID and timestamp to ensure uniqueness
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName =
            $"{fileNamePrefix}{UserID}_{UserName}_{expType.ToString()}_{timestamp}.csv";
        currentFilePath = Path.Combine(folderPath, fileName);

        // Write column headers
        csvBuilder.AppendLine(string.Join(",", columnHeaders));

        // Write header row to file immediately
        try
        {
            File.WriteAllText(currentFilePath, csvBuilder.ToString());
            Debug.Log($"New log file created: {currentFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Failed to create or write file header: {currentFilePath}\nError: {e.Message}"
            );
            currentFilePath = ""; // Creation failed, reset path
        }
    }

    /// <summary>
    /// Log a row of data to current CSV file.
    /// </summary>
    /// <param name="userID">User ID.</param>
    /// <param name="experimentID">Experiment number to which data belongs.</param>
    /// <param name="blockIndex">Block index.</param>
    /// <param name="targetIndex">Target index.</param>
    /// <param name="result">A boolean result.</param>
    public void LogDataRow(
        string userID,
        string experimentType,
        int blockIndex,
        int targetIndex,
        bool result,
        float duration
    )
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            Debug.LogWarning(
                "Log file not yet created or creation failed. Please call CreateNewLogFile first."
            );
            return;
        }

        // Clear StringBuilder for new row (or don't clear and keep appending, depends on your write strategy)
        // For single row append, use a new Builder each time or clear the old one.
        // Since CreateNewLogFile already wrote headers, first LogDataRow should append after headers.
        // Current implementation: headers already written after CreateNewLogFile, so LogDataRow directly appends new rows.

        StringBuilder rowBuilder = new StringBuilder(); // Create a new Builder for current row

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // Timestamp accurate to milliseconds

        // Add data in order of column headers
        rowBuilder.Append(EscapeCSVField(userID)).Append(",");
        rowBuilder.Append(EscapeCSVField(timestamp)).Append(",");
        rowBuilder.Append(EscapeCSVField(experimentType)).Append(",");
        rowBuilder.Append(blockIndex.ToString()).Append(","); // int通常不需要转义
        rowBuilder.Append(targetIndex.ToString()).Append(","); // int通常不需要转义
        if (sceneManager)
        {
            rowBuilder.Append(sceneManager.angularDistance).Append(",");
            rowBuilder.Append(sceneManager.targetVisualWidth_angle).Append(",");
        }
        else if (sceneManager_t2)
        {
            rowBuilder.Append(" ").Append(",");
            rowBuilder.Append(" ").Append(",");
        }

        rowBuilder.Append(result.ToString()).Append(","); // bool usually doesn't need escaping (will output True/False)
        rowBuilder.Append(duration.ToString());
        // Append to file
        try
        {
            // Use StreamWriter to append rows instead of File.AppendAllText each time (latter is slightly less efficient)
            // But for simplicity, use AppendAllText here; optimize later if performance is a bottleneck.
            File.AppendAllText(currentFilePath, rowBuilder.ToString() + Environment.NewLine);
            // Debug.Log("Data row logged."); // May spam output when logging frequently, enable as needed
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to log data row: {currentFilePath}\nError: {e.Message}");
        }
        Debug.Log("add new line to data");
    }

    public void LogDataRow(int blockIndex, int targetIndex, bool result, float duration)
    {
        LogDataRow(
            UserID.ToString(),
            expType.ToString(),
            blockIndex,
            targetIndex,
            result,
            duration
        );
    }

    /// <summary>
    /// Handle special characters in CSV fields (e.g., commas, quotes, newlines).
    /// If field contains comma, quote, or newline, wrap in double quotes and replace internal double quotes with two double quotes.
    /// </summary>
    private string EscapeCSVField(string field)
    {
        if (field == null)
            return "";

        if (
            field.Contains(",")
            || field.Contains("\"")
            || field.Contains("\n")
            || field.Contains("\r")
        )
        {
            // 将字段内的双引号替换为两个双引号
            string escapedField = field.Replace("\"", "\"\"");
            // 用双引号将整个字段括起来
            return $"\"{escapedField}\"";
        }
        return field;
    }

    // ----- 示例用法 (可以放在其他脚本中，或者在这个脚本的Start/Update中测试) -----
    /*
    public DataLogger dataLoggerInstance; // 在Inspector中拖拽赋值，或者GetComponent获取

    void Start()
    {
    // 获取或创建 DataLogger 实例
    dataLoggerInstance = GetComponent<DataLogger>(); // 如果脚本挂在同一个GameObject上
    if (dataLoggerInstance == null)
    {
    dataLoggerInstance = FindObjectOfType<DataLogger>(); // 查找场景中的实例
    if (dataLoggerInstance == null)
    {
    Debug.LogError("DataLogger instance not found!");
    return;
    }
    }


    // 1. 创建一个新的日志文件
    string currentUserID = "User_001"; // 示例用户ID
    dataLoggerInstance.CreateNewLogFile(currentUserID);

    // 2. 记录一些数据
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 1, 0, true);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 1, 1, false);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 2, 0, true);

    // 包含特殊字符的示例
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_B, Condition_Alpha", 3, 2, true);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_C \"Special\"", 4, 1, false);

    }

    void OnApplicationQuit()
    {
    // 可选：在应用退出时，你可能想确保所有缓冲数据都已写入（如果使用了更复杂的缓冲机制）
    // 对于当前的 File.AppendAllText，它会立即写入，所以通常不需要特别处理。
    Debug.Log("应用程序退出，日志记录结束。");
    }
    */
}
