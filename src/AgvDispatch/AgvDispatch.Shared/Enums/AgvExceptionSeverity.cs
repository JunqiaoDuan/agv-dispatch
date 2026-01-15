namespace AgvDispatch.Shared.Enums;

/// <summary>
/// 异常严重级别
/// </summary>
public enum AgvExceptionSeverity
{
    /// <summary>
    /// 提示信息
    /// </summary>
    Info = 10,

    /// <summary>
    /// 警告（可自动恢复）
    /// </summary>
    Warning = 20,

    /// <summary>
    /// 错误（需人工干预）
    /// </summary>
    Error = 30,

    /// <summary>
    /// 严重故障
    /// </summary>
    Critical = 40
}

/// <summary>
/// ExceptionSeverity 扩展方法
/// </summary>
public static class AgvExceptionSeverityExtensions
{
    /// <summary>
    /// 获取严重级别显示文本
    /// </summary>
    public static string ToDisplayText(this AgvExceptionSeverity severity) => severity switch
    {
        AgvExceptionSeverity.Info => "信息",
        AgvExceptionSeverity.Warning => "警告",
        AgvExceptionSeverity.Error => "错误",
        AgvExceptionSeverity.Critical => "严重",
        _ => "未知"
    };
}
