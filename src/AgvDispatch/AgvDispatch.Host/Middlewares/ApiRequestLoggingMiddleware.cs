using System.Diagnostics;
using System.Text;

namespace AgvDispatch.Host.Middlewares;

/// <summary>
/// API 请求/响应日志中间件
/// 记录所有 API 请求的详细信息到 Debug 日志
/// </summary>
public class ApiRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiRequestLoggingMiddleware> _logger;

    public ApiRequestLoggingMiddleware(RequestDelegate next, ILogger<ApiRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 只记录 API 请求（排除静态资源、Blazor 等）
        if (!ShouldLog(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestInfo = await BuildRequestInfoAsync(context.Request);

        // 为了能读取响应内容，需要替换响应流
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // 执行下一个中间件
            await _next(context);

            stopwatch.Stop();

            // 读取响应内容
            var responseInfo = await BuildResponseInfoAsync(context.Response, responseBody);

            // 记录完整的请求/响应日志
            LogHttpRequest(requestInfo, responseInfo, stopwatch.ElapsedMilliseconds);

            // 将响应内容复制回原始流
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP 请求处理异常 | {RequestInfo} | 耗时: {ElapsedMs}ms",
                requestInfo, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// 判断是否需要记录日志
    /// </summary>
    private static bool ShouldLog(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? string.Empty;

        // 只记录 /api 开头的请求
        if (pathValue.StartsWith("/api"))
            return true;

        // 排除其他路径（Blazor、静态资源、Swagger 等）
        return false;
    }

    /// <summary>
    /// 构建请求信息
    /// </summary>
    private static async Task<string> BuildRequestInfoAsync(HttpRequest request)
    {
        var sb = new StringBuilder();
        sb.Append($"{request.Method} {request.Path}{request.QueryString}");

        // 记录查询参数
        if (request.QueryString.HasValue)
        {
            sb.Append($" | Query: {request.QueryString}");
        }

        // 记录重要的请求头（排除敏感信息）
        var headers = GetSafeHeaders(request.Headers);
        if (headers.Any())
        {
            sb.Append($" | Headers: {string.Join(", ", headers.Select(h => $"{h.Key}={h.Value}"))}");
        }

        // 注意：为了安全，不记录请求 Body
        // sb.Append(" | Body: [已省略]");

        return sb.ToString();
    }

    /// <summary>
    /// 构建响应信息
    /// </summary>
    private static async Task<string> BuildResponseInfoAsync(HttpResponse response, MemoryStream responseBody)
    {
        var sb = new StringBuilder();
        sb.Append($"Status: {response.StatusCode}");

        // 记录响应 Body
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        if (!string.IsNullOrWhiteSpace(responseBodyText))
        {
            // 限制响应 Body 长度，避免日志过大
            const int maxBodyLength = 1000;
            var bodyPreview = responseBodyText.Length > maxBodyLength
                ? responseBodyText.Substring(0, maxBodyLength) + "... (已截断)"
                : responseBodyText;
            sb.Append($" | Body: {bodyPreview}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 记录 HTTP 请求日志
    /// </summary>
    private void LogHttpRequest(string requestInfo, string responseInfo, long elapsedMs)
    {
        _logger.LogDebug("HTTP Request | {RequestInfo} => {ResponseInfo} | 耗时: {ElapsedMs}ms",
            requestInfo, responseInfo, elapsedMs);
    }

    /// <summary>
    /// 获取安全的请求头（排除敏感信息）
    /// </summary>
    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        // 敏感请求头黑名单
        var blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Cookie",
            "X-API-Key",
            "X-Auth-Token"
        };

        return headers
            .Where(h => !blacklist.Contains(h.Key))
            .Take(5) // 最多记录 5 个请求头
            .ToDictionary(h => h.Key, h => h.Value.ToString());
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class ApiRequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiRequestLoggingMiddleware>();
    }
}
