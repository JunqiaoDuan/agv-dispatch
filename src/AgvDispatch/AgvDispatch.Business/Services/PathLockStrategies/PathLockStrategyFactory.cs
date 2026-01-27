using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Shared.Options;
using AgvDispatch.Shared.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgvDispatch.Business.Services.PathLockStrategies;

/// <summary>
/// 路径锁定策略工厂
/// </summary>
public class PathLockStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemOptions _systemOptions;
    private readonly ILogger<PathLockStrategyFactory> _logger;

    public PathLockStrategyFactory(
        IServiceProvider serviceProvider,
        IOptions<SystemOptions> systemOptions,
        ILogger<PathLockStrategyFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _systemOptions = systemOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// 根据系统编号创建对应的锁定策略
    /// </summary>
    public IPathLockStrategy CreateStrategy()
    {
        var systemCode = _systemOptions.SystemCode?.ToLower() ?? string.Empty;

        _logger.LogInformation("根据系统编号 '{SystemCode}' 创建路径锁定策略", systemCode);

        return systemCode switch
        {
            "haining" => _serviceProvider.GetRequiredService<HainingPathLockStrategy>(),
            _ => _serviceProvider.GetRequiredService<DefaultPathLockStrategy>()
        };
    }
}
