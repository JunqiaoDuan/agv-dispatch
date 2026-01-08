using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Auth;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 认证客户端接口
/// </summary>
public interface IAuthClient
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request);

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    Task<ApiResponse<UserInfoDto>?> GetCurrentUserAsync();
}

/// <summary>
/// 认证客户端实现
/// </summary>
public class AuthClient : IAuthClient
{
    private readonly HttpClient _httpClient;

    public AuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    }

    public async Task<ApiResponse<UserInfoDto>?> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("api/auth/me");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<ApiResponse<UserInfoDto>>();
    }
}
