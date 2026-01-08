namespace AgvDispatch.Business.Entities.Common;

/// <summary>
/// 具有密码属性的实体接口
/// </summary>
public interface IHasPassword
{
    /// <summary>
    /// 密码哈希
    /// </summary>
    string PasswordHash { get; set; }

    /// <summary>
    /// 密码盐值
    /// </summary>
    string PasswordSalt { get; set; }
}
