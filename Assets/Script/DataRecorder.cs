using UnityEngine;
using System.IO; // 用于文件操作
using System; // 用于获取当前时间
using System.Text; // 用于 StringBuilder

public enum ExpType{ 
    Gaze_Task1,
    SSVEP_Task1,
    Gaze_Task2,
    SSVEP_Task2
}

public class DataRecorder : MonoBehaviour
{
    [Header("用户设置")]
    public int UserID = 0;
    public string UserName = "Test";
    public ExpType expType;

    [Header("场景设置")]
    public sceneManager sceneManager;
    public T2_SceneManager sceneManager_t2;

    [Header("文件设置")]
    [Tooltip("CSV文件保存的相对路径 (相对于项目根目录或打包后的Data文件夹)")]
    private  string relativeFolderPath = "ExperimentData"; // CSV文件保存的相对文件夹路径
    [Tooltip("CSV文件名前缀")]
    private string fileNamePrefix = "Data_"; // CSV文件名前缀

    private string currentFilePath = ""; // 当前正在写入的文件的完整路径
    private StringBuilder csvBuilder = new StringBuilder(); // 用于高效构建CSV行

    // 数据属性 (这些将作为CSV的列标题，也是记录数据时需要传入的)
    // 您可以根据需要调整这些属性的顺序和名称
    private readonly string[] columnHeaders = new string[] {
        "UserID",
        "Timestamp",
        "ExpType",
        "BlockIndex",
        "TargetIndex",
        "PhaseRadius",
        "TargetVisualAngular",
        "Result", // bool 类型
        "SelectionTime"
    };

    /// <summary>
    /// 创建一个新的CSV文件并写入列标题。
    /// 如果文件已存在，则会根据时间戳创建一个新的唯一文件名。
    /// </summary>
    /// <param name="userID">用户ID，将用于文件名的一部分。</param>
    public void CreateNewLogFile()
    {
        // 清空 StringBuilder 以备新文件使用
        csvBuilder.Clear();

        // 确保文件夹存在
        string folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath); // 退回到项目根目录再进入指定文件夹
        if (Application.isEditor) // 在编辑器中，dataPath是Assets文件夹
        {
            folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativeFolderPath);
        }
        else // 在打包的应用中，dataPath是 <AppName>_Data 文件夹
        {
            folderPath = Path.Combine(Application.dataPath, relativeFolderPath);
            // 或者，如果希望在程序可执行文件旁边：
            // folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath);
            // Directory.GetParent(Application.dataPath).FullName 在打包后可能不适用或指向错误位置，
            // Application.persistentDataPath 是一个更可靠的跨平台可写路径选择。
            // 为了简单起见，这里我们先用相对于 Data 文件夹的方式。
            // 对于打包版本，更推荐使用 Application.persistentDataPath
            // folderPath = Path.Combine(Application.persistentDataPath, relativeFolderPath);
        }


        if (!Directory.Exists(folderPath))
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"文件夹创建成功: {folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"创建文件夹失败: {folderPath}\n错误: {e.Message}");
                return;
            }
        }

        // 生成文件名，包含用户ID和时间戳以确保唯一性
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{fileNamePrefix}{UserID}_{UserName}_{expType.ToString()}_{timestamp}.csv";
        currentFilePath = Path.Combine(folderPath, fileName);

        // 写入列标题
        csvBuilder.AppendLine(string.Join(",", columnHeaders));

        // 将标题行立即写入文件
        try
        {
            File.WriteAllText(currentFilePath, csvBuilder.ToString());
            Debug.Log($"新日志文件已创建: {currentFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"创建或写入文件头失败: {currentFilePath}\n错误: {e.Message}");
            currentFilePath = ""; // 创建失败，重置路径
        }
    }


    /// <summary>
    /// 向当前CSV文件记录一行数据。
    /// </summary>
    /// <param name="userID">用户ID。</param>
    /// <param name="experimentID">数据所属的实验标号。</param>
    /// <param name="blockIndex">Block的索引。</param>
    /// <param name="targetIndex">Target的索引。</param>
    /// <param name="result">一个布尔类型的结果。</param>
    public void LogDataRow(string userID, string experimentType, int blockIndex, int targetIndex, bool result, float duration)
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            Debug.LogWarning("日志文件尚未创建或创建失败。请先调用 CreateNewLogFile。");
            return;
        }

        // 清空 StringBuilder 以备新行使用 (或者不清空，继续Append，取决于你的写入策略)
        // 为了单行追加，每次都用新的Builder或者清空旧的。
        // 但如果CreateNewLogFile后立即追加，标题行已经在csvBuilder里了，所以第一次LogDataRow应该追加到标题后。
        // 当前实现是CreateNewLogFile后标题已写入，所以LogDataRow直接追加新行。

        StringBuilder rowBuilder = new StringBuilder(); // 为当前行创建一个新的Builder

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // 精确到毫秒的时间戳

        // 按照列标题的顺序添加数据
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
        
        rowBuilder.Append(result.ToString()).Append(","); // bool通常不需要转义 (会输出 True/False)
        rowBuilder.Append(duration.ToString());
        // 追加到文件
        try
        {
            // 使用 StreamWriter 来追加行，而不是每次都用 File.AppendAllText (后者效率稍低)
            // 但为了简单，这里先用 AppendAllText，如果性能是瓶颈再优化。
            File.AppendAllText(currentFilePath, rowBuilder.ToString() + Environment.NewLine);
            // Debug.Log("数据行已记录。"); // 频繁记录时可能会刷屏，按需开启
        }
        catch (Exception e)
        {
            Debug.LogError($"记录数据行失败: {currentFilePath}\n错误: {e.Message}");
        }
        Debug.Log("add new line to data");
    }

    public void LogDataRow(int blockIndex, int targetIndex, bool result, float duration)
    {
        LogDataRow(UserID.ToString(), expType.ToString(), blockIndex, targetIndex, result, duration);
    }

    /// <summary>
    /// 处理CSV字段中的特殊字符（例如逗号、引号、换行符）。
    /// 如果字段包含逗号、引号或换行符，则用双引号括起来，并将字段内的双引号替换为两个双引号。
    /// </summary>
    private string EscapeCSVField(string field)
    {
        if (field == null) return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
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


