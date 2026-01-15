namespace AgvDispatch.Shared.Constants;

/// <summary>
/// MQTT Topic 常量定义
/// </summary>
public static class MqttTopics
{

    #region Topics： AGV => 服务器

    /// <summary>
    /// 状态上报 Topic 模板: agv/{agvCode}/status
    /// </summary>
    public const string StatusTemplate = "agv/{0}/status";

    /// <summary>
    /// 任务进度 Topic 模板: agv/{agvCode}/task/progress
    /// </summary>
    public const string TaskProgressTemplate = "agv/{0}/task/progress";

    /// <summary>
    /// 异常上报 Topic 模板: agv/{agvCode}/exception
    /// </summary>
    public const string ExceptionTemplate = "agv/{0}/exception";

    #endregion

    #region Topics： 服务器 => AGV

    /// <summary>
    /// 任务下发 Topic 模板: agv/{agvCode}/task/assign
    /// </summary>
    public const string TaskAssignTemplate = "agv/{0}/task/assign";

    /// <summary>
    /// 取消任务 Topic 模板: agv/{agvCode}/task/cancel
    /// </summary>
    public const string TaskCancelTemplate = "agv/{0}/task/cancel";

    /// <summary>
    /// 控制指令 Topic 模板: agv/{agvCode}/command
    /// </summary>
    public const string CommandTemplate = "agv/{0}/command";

    #endregion

    #region 服务器订阅用的通配符 Topic

    /// <summary>
    /// 订阅所有小车状态: agv/+/status
    /// </summary>
    public const string AllStatus = "agv/+/status";

    /// <summary>
    /// 订阅所有任务进度: agv/+/task/progress
    /// </summary>
    public const string AllTaskProgress = "agv/+/task/progress";

    /// <summary>
    /// 订阅所有异常: agv/+/exception
    /// </summary>
    public const string AllException = "agv/+/exception";

    #endregion

    #region 命令字符串生成器

    /// <summary>
    /// 生成指定小车的状态 Topic
    /// </summary>
    public static string Status(string agvCode) => string.Format(StatusTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的任务下发 Topic
    /// </summary>
    public static string TaskAssign(string agvCode) => string.Format(TaskAssignTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的取消任务 Topic
    /// </summary>
    public static string TaskCancel(string agvCode) => string.Format(TaskCancelTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的任务进度 Topic
    /// </summary>
    public static string TaskProgress(string agvCode) => string.Format(TaskProgressTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的控制指令 Topic
    /// </summary>
    public static string Command(string agvCode) => string.Format(CommandTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的异常上报 Topic
    /// </summary>
    public static string Exception(string agvCode) => string.Format(ExceptionTemplate, agvCode);

    #endregion

}
