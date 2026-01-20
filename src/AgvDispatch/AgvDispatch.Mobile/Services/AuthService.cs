using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Auth;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _token;
    private UserInfoDto? _currentUser;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public string? GetToken() => _token;

    public UserInfoDto? GetCurrentUser() => _currentUser;

    public async Task<(bool Success, string? Message)> LoginAsync(string username, string password)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AgvApi");
            var response = await client.PostAsJsonAsync("api/auth/login", new LoginRequest
            {
                Username = username,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    _token = result.Data.Token;
                    _currentUser = result.Data.User;
                    return (true, null);
                }
                else
                {
                    return (false, result?.Message ?? "登录失败");
                }
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return (false, error?.Message ?? "登录失败");
            }
        }
        catch (Exception ex)
        {
            return (false, $"网络错误: {ex.Message}");
        }
    }

    public Task LogoutAsync()
    {
        _token = null;
        _currentUser = null;
        return Task.CompletedTask;
    }
}
