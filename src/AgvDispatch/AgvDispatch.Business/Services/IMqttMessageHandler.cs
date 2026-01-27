using AgvDispatch.Shared.Messages;

namespace AgvDispatch.Business.Services;

/// <summary>
/// MQTT 消息处理器接口
/// </summary>
public interface IMqttMessageHandler
{
    /// <summary>
    /// 处理状态上报消息
    /// </summary>
    Task HandleStatusAsync(string agvCode, StatusMessage message);

    /// <summary>
    /// 处理任务进度消息
    /// </summary>
    Task HandleTaskProgressAsync(string agvCode, TaskProgressMessage message);

    /// <summary>
    /// 处理异常上报消息
    /// </summary>
    Task HandleExceptionAsync(string agvCode, ExceptionMessage message);

    /// <summary>
    /// 处理路径锁定请求消息
    /// </summary>
    Task HandlePathLockRequestAsync(string agvCode, PathLockRequestMessage message);

}
