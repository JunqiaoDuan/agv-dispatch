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

    /// <summary>
    /// 路段锁定请求 Topic 模板: agv/{agvCode}/path/lock-request
    /// </summary>
    public const string PathLockRequestTemplate = "agv/{0}/path/lock-request";

    /// <summary>
    /// 路段解锁通知 Topic 模板: agv/{agvCode}/path/unlock
    /// </summary>
    public const string PathUnlockTemplate = "agv/{0}/path/unlock";

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

    /// <summary>
    /// 路段锁定响应 Topic 模板: agv/{agvCode}/path/lock-response
    /// </summary>
    public const string PathLockResponseTemplate = "agv/{0}/path/lock-response";

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

    /// <summary>
    /// 订阅所有路段锁定请求: agv/+/path/lock-request
    /// </summary>
    public const string AllPathLockRequest = "agv/+/path/lock-request";

    /// <summary>
    /// 订阅所有路段解锁通知: agv/+/path/unlock
    /// </summary>
    public const string AllPathUnlock = "agv/+/path/unlock";

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

    /// <summary>
    /// 生成指定小车的路段锁定请求 Topic
    /// </summary>
    public static string PathLockRequest(string agvCode) => string.Format(PathLockRequestTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的路段锁定响应 Topic
    /// </summary>
    public static string PathLockResponse(string agvCode) => string.Format(PathLockResponseTemplate, agvCode);

    /// <summary>
    /// 生成指定小车的路段解锁 Topic
    /// </summary>
    public static string PathUnlock(string agvCode) => string.Format(PathUnlockTemplate, agvCode);

    #endregion

    #region 消息类型常量

    /// <summary>
    /// 消息类型: 状态上报
    /// </summary>
    public const string MessageTypeStatus = "status";

    /// <summary>
    /// 消息类型: 任务进度
    /// </summary>
    public const string MessageTypeTaskProgress = "task/progress";

    /// <summary>
    /// 消息类型: 异常上报
    /// </summary>
    public const string MessageTypeException = "exception";

    /// <summary>
    /// 消息类型: 任务下发
    /// </summary>
    public const string MessageTypeTaskAssign = "task/assign";

    /// <summary>
    /// 消息类型: 取消任务
    /// </summary>
    public const string MessageTypeTaskCancel = "task/cancel";

    /// <summary>
    /// 消息类型: 控制指令
    /// </summary>
    public const string MessageTypeCommand = "command";

    /// <summary>
    /// 消息类型: 路段锁定请求
    /// </summary>
    public const string MessageTypePathLockRequest = "path/lock-request";

    /// <summary>
    /// 消息类型: 路段锁定响应
    /// </summary>
    public const string MessageTypePathLockResponse = "path/lock-response";

    #endregion

    #region Topic 解析

    /// <summary>
    /// 解析 Topic，提取 AgvCode 和消息类型
    /// Topic 格式: agv/{agvCode}/{messageType}
    /// </summary>
    /// <param name="topic">MQTT Topic</param>
    /// <returns>agvCode: 小车编号, messageType: 消息类型 (status, task/progress, exception 等)</returns>
    public static (string? AgvCode, string? MessageType) ParseTopic(string topic)
    {
        if (string.IsNullOrEmpty(topic))
        {
            return (null, null);
        }

        var parts = topic.Split('/');

        if (parts.Length >= 3 && parts[0] == "agv" && !string.IsNullOrEmpty(parts[1]))
        {
            var agvCode = parts[1];
            var messageType = string.Join("/", parts.Skip(2));
            return (agvCode, messageType);
        }

        return (null, null);
    }

    #endregion

}
