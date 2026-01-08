using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgvDispatch.Business.Entities.Common;
using AgvDispatch.Business.Entities.UserAggregate;
using AgvDispatch.Business.Specifications.Users;
using AgvDispatch.Shared.DTOs.Auth;
using AgvDispatch.Shared.Options;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AgvDispatch.Host.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IReadRepository<User> _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IReadRepository<User> userRepository,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // 查找用户
        var spec = new UserByUsernameSpec(request.Username);
        var user = await _userRepository.FirstOrDefaultAsync(spec);

        if (user == null)
        {
            _logger.LogWarning("登录失败：用户 {Username} 不存在", request.Username);
            return null;
        }

        // 验证密码
        if (!user.VerifyPassword(request.Password))
        {
            _logger.LogWarning("登录失败：用户 {Username} 密码错误", request.Username);
            return null;
        }

        // 生成 Token
        var expiresAt = DateTimeOffset.UtcNow.AddHours(_jwtOptions.ExpirationHours);
        var token = GenerateJwtToken(user, expiresAt);

        _logger.LogInformation("用户 {Username} 登录成功", request.Username);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role
            }
        };
    }

    /// <inheritdoc />
    public async Task<UserInfoDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsValid)
        {
            return null;
        }

        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(User user, DateTimeOffset expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
