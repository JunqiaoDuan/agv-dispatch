using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 自动添加 JWT Token 到 HTTP 请求头的消息处理器
/// 
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthStateService _authStateService;
    private readonly ILogger<AuthorizationMessageHandler> _logger;

    public AuthorizationMessageHandler(IAuthStateService authStateService, ILogger<AuthorizationMessageHandler> logger)
    {
        _authStateService = authStateService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authStateService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AuthHandler] 获取 Token 失败");
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("[AuthHandler] API 返回 401: {Uri}", request.RequestUri);
        }

        return response;
    }
}
