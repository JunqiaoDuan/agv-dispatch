using AgvDispatch.Business.Entities.Common;

namespace AgvDispatch.Business.Entities.UserAggregate;

/// <summary>
/// 系统用户实体
/// </summary>
public class User : BaseEntity, IHasPassword
{
    /// <summary>
    /// 用户名（唯一）
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 密码盐值
    /// </summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// 角色：Admin | User
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }
}
