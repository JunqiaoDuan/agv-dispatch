namespace AgvDispatch.Infrastructure.Options;

/// <summary>
/// 健康检测配置选项
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// AGV 离线阈值（秒）
    /// 如果小车在此时间内未发送消息，则标记为离线
    /// </summary>
    public int AgvOfflineThresholdSeconds { get; set; } = 60;

    /// <summary>
    /// 检查间隔（秒）
    /// 定时任务执行的频率
    /// </summary>
    public int CheckIntervalSeconds { get; set; } = 30;
}
