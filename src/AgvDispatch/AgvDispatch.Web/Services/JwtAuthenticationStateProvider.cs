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

    // 初始化标志和锁，确保恢复过程只执行一次
    private static bool _isInitialized = false;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

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
        _isInitialized = true;

        await _localStorage.SetAsync(TokenKey, loginResponse.Token);
        await _localStorage.SetAsync(UserKey, loginResponse.User);
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("[AuthState] 用户登出");

        // 清除静态缓存
        _staticCachedToken = null;
        _staticCachedUser = null;
        _isInitialized = false;

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

        // 如果静态缓存为空且未初始化，尝试从持久化存储恢复
        if (!_isInitialized)
        {
            await _initLock.WaitAsync();
            try
            {
                // 双重检查，防止并发初始化
                if (!_isInitialized)
                {
                    _logger.LogInformation("[AuthState] 尝试从 ProtectedLocalStorage 恢复 Token");
                    await InitializeFromStorageAsync();
                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthState] 从存储恢复 Token 失败");
            }
            finally
            {
                _initLock.Release();
            }
        }

        return _staticCachedToken;
    }

    /// <summary>
    /// 从 ProtectedLocalStorage 初始化静态缓存
    /// </summary>
    private async Task InitializeFromStorageAsync()
    {
        try
        {
            var tokenResult = await _localStorage.GetAsync<string>(TokenKey);
            var userResult = await _localStorage.GetAsync<UserInfoDto>(UserKey);

            if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
            {
                _staticCachedToken = tokenResult.Value;
                _logger.LogInformation("[AuthState] Token 已从存储恢复");
            }

            if (userResult.Success && userResult.Value != null)
            {
                _staticCachedUser = userResult.Value;
                _logger.LogInformation("[AuthState] 用户信息已从存储恢复: {Username}", userResult.Value.Username);
            }
        }
        catch (InvalidOperationException)
        {
            // JS interop 不可用，可能在预渲染期间
            _logger.LogDebug("[AuthState] JS interop 不可用，跳过恢复");
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        // 优先从静态缓存获取
        if (_staticCachedUser != null)
        {
            return _staticCachedUser;
        }

        // 如果静态缓存为空且未初始化，尝试从持久化存储恢复
        if (!_isInitialized)
        {
            await _initLock.WaitAsync();
            try
            {
                // 双重检查，防止并发初始化
                if (!_isInitialized)
                {
                    _logger.LogInformation("[AuthState] 尝试从 ProtectedLocalStorage 恢复用户信息");
                    await InitializeFromStorageAsync();
                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthState] 从存储恢复用户信息失败");
            }
            finally
            {
                _initLock.Release();
            }
        }

        return _staticCachedUser;
    }
}
