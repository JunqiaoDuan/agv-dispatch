using AgvDispatch.Shared.DTOs.Auth;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    bool IsAuthenticated { get; }
    string? GetToken();
    Task<(bool Success, string? Message)> LoginAsync(string username, string password);
    Task LogoutAsync();
    UserInfoDto? GetCurrentUser();
}
