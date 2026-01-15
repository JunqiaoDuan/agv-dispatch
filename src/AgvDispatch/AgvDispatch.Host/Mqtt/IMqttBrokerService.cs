using AgvDispatch.Shared.Messages;
using AgvDispatch.Shared.Constants;
using System.Text.Json;

namespace AgvDispatch.Host.Mqtt;

/// <summary>
/// MQTT 服务接口
/// </summary>
public interface IMqttBrokerService
{
    /// <summary>
    /// 发布任务下发消息
    /// </summary>
    Task PublishTaskAssignAsync(string agvCode, TaskAssignMessage message);

    /// <summary>
    /// 发布取消任务消息
    /// </summary>
    Task PublishTaskCancelAsync(string agvCode, TaskCancelMessage message);

    /// <summary>
    /// 发布控制指令消息
    /// </summary>
    Task PublishCommandAsync(string agvCode, CommandMessage message);
}
