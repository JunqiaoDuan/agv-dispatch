using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.MqttMessageLogAggregate;

/// <summary>
/// MQTT消息日志实体
/// </summary>
public class MqttMessageLog : BaseEntity
{
    /// <summary>
    /// 消息时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// MQTT Topic
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// 消息负载内容（JSON格式）
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// 客户端ID（发送者）
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// QoS级别（0/1/2）
    /// </summary>
    public int Qos { get; set; }

    /// <summary>
    /// 消息方向（Inbound=接收, Outbound=发送）
    /// </summary>
    public MqttMessageDirection Direction { get; set; }

    /// <summary>
    /// AGV编号（从Topic中解析）
    /// </summary>
    public string? AgvCode { get; set; }

    /// <summary>
    /// 消息类型（从Topic中解析，如status、task/assign等）
    /// </summary>
    public string? MessageType { get; set; }
}

/// <summary>
/// MQTT消息方向
/// </summary>
public enum MqttMessageDirection
{
    /// <summary>
    /// 入站消息（AGV→服务器）
    /// </summary>
    Inbound = 1,

    /// <summary>
    /// 出站消息（服务器→AGV）
    /// </summary>
    Outbound = 2
}
