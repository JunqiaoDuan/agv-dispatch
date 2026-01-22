using AgvDispatch.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace AgvDispatch.Web;

public static class WebServiceExtensions
{
    /// <summary>
    /// 注册 Blazor Web UI 所需的服务
    /// </summary>
    public static IServiceCollection AddWebUI(this IServiceCollection services, string apiBaseUrl)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();

        // 注册 Token 管理服务（用于存储和获取 JWT Token）
        services.AddScoped<IAuthStateService, AuthStateService>();

        // 注册未授权重定向服务（用于401时跳转到登录页，使用Singleton确保全局共享）
        services.AddSingleton<IUnauthorizedRedirectService, UnauthorizedRedirectService>();

        // 注册授权消息处理器（自动附加 JWT Token 到 API 请求）
        services.AddScoped<AuthorizationMessageHandler>();

        // 添加 HttpClient 用于调用后端 API
        services.AddHttpClient("AgvApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();

        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AgvApi"));

        // 注册业务服务
        services.AddScoped<IAgvClient, AgvClient>();
        services.AddScoped<IAuthClient, AuthClient>();
        services.AddScoped<IMapClient, MapClient>();
        services.AddScoped<IRouteClient, RouteClient>();
        services.AddScoped<IStationClient, StationClient>();
        services.AddScoped<IMqttMessageClient, MqttMessageClient>();
        services.AddScoped<IBackgroundJobLogClient, BackgroundJobLogClient>();
        services.AddScoped<ITaskClient, TaskClient>();

        return services;
    }

    /// <summary>
    /// 配置 Blazor Web UI 中间件
    /// </summary>
    public static WebApplication UseWebUI<TApp>(this WebApplication app) where TApp : Microsoft.AspNetCore.Components.IComponent
    {
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<TApp>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
