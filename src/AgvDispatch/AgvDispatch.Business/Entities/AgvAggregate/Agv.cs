using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Entities.AgvAggregate;

/// <summary>
/// AGV小车实体
/// </summary>
public class Agv : BaseEntity, IHasPassword
{
    /// <summary>
    /// 小车编号，如 V001
    /// </summary>
    public string AgvCode { get; set; } = string.Empty;

    /// <summary>
    /// 小车名称，如 1号车
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// MQTT 连接密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 密码盐值
    /// </summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// 运行状态
    /// </summary>
    public AgvStatus AgvStatus { get; set; } = AgvStatus.Offline;

    /// <summary>
    /// 电量百分比 (0-100)
    /// </summary>
    public int Battery { get; set; } = 100;

    /// <summary>
    /// 当前速度 (m/s)
    /// </summary>
    public decimal Speed { get; set; } = 0;

    /// <summary>
    /// X坐标 (厘米)
    /// </summary>
    public decimal PositionX { get; set; }

    /// <summary>
    /// Y坐标 (厘米)
    /// </summary>
    public decimal PositionY { get; set; }

    /// <summary>
    /// 朝向角度 (0-360度，正东为0)
    /// </summary>
    public decimal PositionAngle { get; set; }

    /// <summary>
    /// 当前所在站点ID
    /// </summary>
    public Guid? CurrentStationId { get; set; }

    /// <summary>
    /// 当前执行的任务ID
    /// </summary>
    public Guid? CurrentTaskId { get; set; }

    /// <summary>
    /// 是否有料（是否携带物料）
    /// </summary>
    public bool HasCargo { get; set; } = false;

    /// <summary>
    /// 错误码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 最后在线时间
    /// </summary>
    public DateTimeOffset? LastOnlineTime { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
