using AgvDispatch.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Web.Services;

/// <summary>
/// Token 管理服务接口
/// 用于 Blazor Server 应用中管理用户认证状态
/// </summary>
public interface IAuthStateService
{
    /// <summary>
    /// 登录并保存 Token
    /// </summary>
    Task LoginAsync(LoginResponse loginResponse);

    /// <summary>
    /// 登出
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// 获取当前 Token
    /// </summary>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    Task<UserInfoDto?> GetCurrentUserAsync();
}

/// <summary>
/// Token 管理服务实现
/// </summary>
public class AuthStateService : IAuthStateService
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<AuthStateService> _logger;
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";

    // 使用静态缓存，解决 HttpClientFactory 实例隔离问题
    private static string? _staticCachedToken;
    private static UserInfoDto? _staticCachedUser;

    public AuthStateService(ProtectedLocalStorage localStorage, ILogger<AuthStateService> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task LoginAsync(LoginResponse loginResponse)
    {
        _logger.LogInformation("[AuthState] 用户登录: {Username}", loginResponse.User.Username);

        // 保存到静态缓存
        _staticCachedToken = loginResponse.Token;
        _staticCachedUser = loginResponse.User;

        await _localStorage.SetAsync(TokenKey, loginResponse.Token);
        await _localStorage.SetAsync(UserKey, loginResponse.User);
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("[AuthState] 用户登出");

        // 清除静态缓存
        _staticCachedToken = null;
        _staticCachedUser = null;

        await _localStorage.DeleteAsync(TokenKey);
        await _localStorage.DeleteAsync(UserKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        // 优先从静态缓存获取
        if (!string.IsNullOrEmpty(_staticCachedToken))
        {
            return _staticCachedToken;
        }

        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _staticCachedToken = result.Value;
                return result.Value;
            }
            return null;
        }
        catch (InvalidOperationException)
        {
            // JS interop 不可用时返回静态缓存（可能为null）
            return _staticCachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthState] 获取 Token 异常");
            return _staticCachedToken;
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        if (_staticCachedUser != null)
        {
            return _staticCachedUser;
        }

        try
        {
            var result = await _localStorage.GetAsync<UserInfoDto>(UserKey);
            if (result.Success)
            {
                _staticCachedUser = result.Value;
                return result.Value;
            }
            return null;
        }
        catch
        {
            return _staticCachedUser;
        }
    }
}
