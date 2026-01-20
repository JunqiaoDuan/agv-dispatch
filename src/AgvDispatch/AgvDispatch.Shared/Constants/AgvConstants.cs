namespace AgvDispatch.Shared.Constants;

/// <summary>
/// AGV相关常量定义
/// </summary>
public static class AgvConstants
{
    #region 电池电压配置

    /// <summary>
    /// 电池最低电压 (V) - 对应0%电量
    /// </summary>
    public const decimal MinBatteryVoltage = 46.0m;

    /// <summary>
    /// 电池最高电压 (V) - 对应100%电量
    /// </summary>
    public const decimal MaxBatteryVoltage = 53.0m;

    #endregion

    #region 电压到百分比转换

    /// <summary>
    /// 根据电池电压计算电量百分比
    /// </summary>
    /// <param name="voltage">电池电压 (V)</param>
    /// <returns>电量百分比 (0-100)</returns>
    public static int CalculateBatteryPercentage(decimal voltage)
    {
        if (voltage <= MinBatteryVoltage)
            return 0;

        if (voltage >= MaxBatteryVoltage)
            return 100;

        // 线性映射: (voltage - min) / (max - min) * 100
        var percentage = (voltage - MinBatteryVoltage) / (MaxBatteryVoltage - MinBatteryVoltage) * 100;

        // 四舍五入并确保在0-100范围内
        return Math.Clamp((int)Math.Round(percentage), 0, 100);
    }

    #endregion
}
