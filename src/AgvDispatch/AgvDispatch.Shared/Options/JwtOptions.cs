namespace AgvDispatch.Shared.Options;

/// <summary>
/// JWT 配置选项
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// 密钥（至少32位）
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 签发者
    /// </summary>
    public string Issuer { get; set; } = "AgvDispatch";

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = "AgvDispatchClients";

    /// <summary>
    /// Token 有效期（小时）
    /// </summary>
    public int ExpirationHours { get; set; } = 10;
}
