using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Entities.TaskPathLockAggregate;
using AgvDispatch.Business.Specifications.Agvs;
using AgvDispatch.Business.Specifications.PathLocks;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 路径锁定服务实现
/// </summary>
public class PathLockService : IPathLockService
{
    private readonly IRepository<TaskPathLock> _lockRepository;
    private readonly IRepository<Agv> _agvRepository;
    private readonly ILogger<PathLockService> _logger;

    public PathLockService(
        IRepository<TaskPathLock> lockRepository,
        IRepository<Agv> agvRepository,
        ILogger<PathLockService> logger)
    {
        _lockRepository = lockRepository;
        _agvRepository = agvRepository;
        _logger = logger;
    }

    public async Task<(bool Approved, string? Reason)> RequestLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode,
        Guid taskId)
    {
        throw new NotImplementedException();
    }

    public async Task ReleaseLockAsync(
        string fromStationCode,
        string toStationCode,
        string agvCode)
    {
        throw new NotImplementedException();
    }

    public async Task ClearAgvLocksAsync(string agvCode)
    {
        throw new NotImplementedException();
    }
}
