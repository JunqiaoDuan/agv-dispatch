using System.Security.Cryptography;

namespace AgvDispatch.Business.Entities.Common;

/// <summary>
/// 密码扩展方法
/// </summary>
public static class PasswordExtensions
{
    /// <summary>
    /// 设置密码（自动生成盐值并哈希）
    /// </summary>
    public static void SetPassword(this IHasPassword entity, string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(32);
        entity.PasswordSalt = Convert.ToBase64String(saltBytes);
        entity.PasswordHash = HashPassword(password, saltBytes);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    public static bool VerifyPassword(this IHasPassword entity, string password)
    {
        if (string.IsNullOrEmpty(entity.PasswordSalt) || string.IsNullOrEmpty(entity.PasswordHash))
            return false;

        var saltBytes = Convert.FromBase64String(entity.PasswordSalt);
        var hash = HashPassword(password, saltBytes);
        return hash == entity.PasswordHash;
    }

    private static string HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hashBytes = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hashBytes);
    }
}
