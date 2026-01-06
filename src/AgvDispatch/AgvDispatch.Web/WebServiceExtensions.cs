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

        // 添加 HttpClient 用于调用后端 API
        services.AddHttpClient("AgvApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AgvApi"));

        // 注册业务服务
        services.AddScoped<IAgvClient, AgvClient>();

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
