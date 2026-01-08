using AgvDispatch.Shared.DTOs.Auth;

namespace AgvDispatch.Host.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    Task<UserInfoDto?> GetCurrentUserAsync(Guid userId);
}
