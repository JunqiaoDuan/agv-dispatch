namespace AgvDispatch.Shared.Constants;

/// <summary>
/// 任务相关常量定义
/// </summary>
public static class TaskConstants
{
    #region AGV推荐配置

    /// <summary>
    /// AGV推荐时的最低电量要求 (%)
    /// </summary>
    public const int MinBatteryForRecommendation = 20;

    /// <summary>
    /// AGV推荐列表最大返回数量
    /// </summary>
    public const int RecommendationTopCount = 10;

    #endregion
}
