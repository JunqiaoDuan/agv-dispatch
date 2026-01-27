using AgvDispatch.Business.Services;
using AgvDispatch.Business.Services.PathLockStrategies;
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

        // 任务路径规划服务
        services.AddScoped<ITaskRouteService, TaskRouteService>();

        // 小车推荐服务
        services.AddScoped<IAgvRecommendationService, AgvRecommendationService>();

        // 路径锁定服务及策略
        services.AddScoped<IPathLockService, PathLockService>();
        services.AddScoped<PathLockStrategyFactory>();
        services.AddScoped<DefaultPathLockStrategy>();
        services.AddScoped<HainingPathLockStrategy>();

        return services;
    }
}
