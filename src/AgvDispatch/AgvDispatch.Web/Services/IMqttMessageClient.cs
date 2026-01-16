using AgvDispatch.Business.Entities.MqttMessageLogAggregate;
using AgvDispatch.Shared.DTOs;

namespace AgvDispatch.Web.Services;

/// <summary>
/// MQTT消息API客户端接口
/// </summary>
public interface IMqttMessageClient
{
    /// <summary>
    /// 按时间范围分页查询MQTT消息
    /// </summary>
    /// <param name="startTime">起始时间 (格式: yyyyMMdd)</param>
    /// <param name="endTime">结束时间 (格式: yyyyMMdd)</param>
    Task<PagedResponse<MqttMessageLog>?> GetByTimeRangeAsync(
        string? startTime = null,
        string? endTime = null,
        string? agvCode = null,
        int? direction = null,
        string? messageType = null,
        int pageIndex = 0,
        int pageSize = 50);

    /// <summary>
    /// 获取消息统计信息
    /// </summary>
    /// <param name="since">起始日期 (格式: yyyyMMdd)</param>
    Task<MqttMessageStatistics?> GetStatisticsAsync(string? since = null);
}

/// <summary>
/// MQTT消息统计信息
/// </summary>
public class MqttMessageStatistics
{
    public int TotalCount { get; set; }
    public int InboundCount { get; set; }
    public int OutboundCount { get; set; }
    public List<AgvMessageCount> ByAgv { get; set; } = new();
    public List<MessageTypeCount> ByMessageType { get; set; } = new();
}

public class AgvMessageCount
{
    public string? AgvCode { get; set; }
    public int Count { get; set; }
}

public class MessageTypeCount
{
    public string? MessageType { get; set; }
    public int Count { get; set; }
}
