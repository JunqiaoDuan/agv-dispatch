using AgvDispatch.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgvDispatch.Business.Extensions;

/// <summary>
/// 任务服务扩展
/// </summary>
public static class TaskServicesExtensions
{
    /// <summary>
    /// 添加任务相关服务
    /// </summary>
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        // 任务管理服务
        services.AddScoped<ITaskJobService, TaskJobService>();

        // 小车推荐服务
        services.AddScoped<IAgvRecommendationService, AgvRecommendationService>();

        return services;
    }
}
