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

    public AuthStateService(ProtectedLocalStorage localStorage, ILogger<AuthStateService> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task LoginAsync(LoginResponse loginResponse)
    {
        _logger.LogInformation("[AuthState] 用户登录: {Username}", loginResponse.User.Username);

        await _localStorage.SetAsync(TokenKey, loginResponse.Token);
        await _localStorage.SetAsync(UserKey, loginResponse.User);
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("[AuthState] 用户登出");

        await _localStorage.DeleteAsync(TokenKey);
        await _localStorage.DeleteAsync(UserKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            if (result.Success)
            {
                _logger.LogDebug("[AuthState] 成功获取 Token");
            }
            return result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            // 预渲染期间无法访问 LocalStorage，这是正常情况
            _logger.LogDebug("[AuthState] 预渲染期间无法访问 LocalStorage");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuthState] 获取 Token 异常: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<UserInfoDto>(UserKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }
}
