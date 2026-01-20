using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.AgvAggregate;

/// <summary>
/// AGV异常日志实体
/// 设计原则：存储MQTT消息原样 + 最小必要的业务字段
/// </summary>
public class AgvExceptionLog : BaseEntity
{
    // ==================== MQTT消息原样字段 ====================

    /// <summary>
    /// 小车编号 (来自MQTT消息)
    /// </summary>
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 关联的任务ID (来自MQTT消息，字符串格式，可选)
    /// </summary>
    public string? TaskId { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    public AgvExceptionType ExceptionType { get; set; }

    /// <summary>
    /// 严重级别
    /// </summary>
    public AgvExceptionSeverity Severity { get; set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 发生时的X坐标 (厘米，来自MQTT消息的position，可能为null)
    /// </summary>
    public decimal? PositionX { get; set; }

    /// <summary>
    /// 发生时的Y坐标 (厘米，来自MQTT消息的position，可能为null)
    /// </summary>
    public decimal? PositionY { get; set; }

    /// <summary>
    /// 发生时的朝向角度 (0-360度，来自MQTT消息的position，可能为null)
    /// </summary>
    public decimal? PositionAngle { get; set; }

    /// <summary>
    /// 发生时所在站点ID (来自MQTT消息的position.stationId，字符串格式，可能为null)
    /// </summary>
    public string? StationCode { get; set; }

    /// <summary>
    /// 异常发生时间
    /// </summary>
    public DateTimeOffset ExceptionTime { get; set; }

    // ==================== 业务必要字段 ====================

    /// <summary>
    /// 错误码 (系统生成，用于标识和追溯此次异常)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 是否已处理
    /// </summary>
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTimeOffset? ResolvedTime { get; set; }

    /// <summary>
    /// 处理备注
    /// </summary>
    public string? ResolvedRemark { get; set; }
}
