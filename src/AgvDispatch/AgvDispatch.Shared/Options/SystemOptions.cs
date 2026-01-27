namespace AgvDispatch.Shared.Options;

/// <summary>
/// 系统配置选项
/// </summary>
public class SystemOptions
{
    /// <summary>
    /// 系统编号（用于区分不同项目的专用逻辑）
    /// </summary>
    public string SystemCode { get; set; } = string.Empty;
}
