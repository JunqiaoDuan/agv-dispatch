using System.Net;
using System.Text.Json;
using AgvDispatch.Shared.DTOs;

namespace AgvDispatch.Host.Middlewares;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "请求处理过程中发生未处理的异常: {Path}", context.Request.Path);

        var statusCode = GetStatusCode(exception);
        var message = GetErrorMessage(exception);

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ArgumentException => (int)HttpStatusCode.BadRequest,
        KeyNotFoundException => (int)HttpStatusCode.NotFound,
        UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
        InvalidOperationException => (int)HttpStatusCode.BadRequest,
        NotSupportedException => (int)HttpStatusCode.BadRequest,
        OperationCanceledException => 499, // Client Closed Request
        _ => (int)HttpStatusCode.InternalServerError
    };

    private string GetErrorMessage(Exception exception) => exception switch
    {
        ArgumentException => exception.Message,
        KeyNotFoundException => "请求的资源不存在",
        UnauthorizedAccessException => "未授权的访问",
        InvalidOperationException => exception.Message,
        NotSupportedException => "不支持的操作",
        OperationCanceledException => "请求已取消",
        _ => _environment.IsDevelopment() ? exception.Message : "服务器内部错误，请稍后重试"
    };
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
