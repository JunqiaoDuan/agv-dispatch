using AgvDispatch.Business.Services;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.PathLocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PathLocksController : ControllerBase
{
    private readonly IPathLockService _pathLockService;
    private readonly ILogger<PathLocksController> _logger;

    public PathLocksController(
        IPathLockService pathLockService,
        ILogger<PathLocksController> logger)
    {
        _pathLockService = pathLockService;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前已放行的通道列表
    /// </summary>
    [HttpGet("active-channels")]
    public async Task<ActionResult<ApiResponse<List<ActiveChannelDto>>>> GetActiveChannels()
    {
        var channels = await _pathLockService.GetActiveChannelsAsync();
        _logger.LogInformation("[PathLocksController] 获取已放行通道列表成功: 数量={Count}", channels.Count);
        return Ok(ApiResponse<List<ActiveChannelDto>>.Ok(channels));
    }

    /// <summary>
    /// 获取指定通道的详细信息
    /// </summary>
    /// <param name="channelName">通道名称</param>
    [HttpGet("channel/{channelName}")]
    public async Task<ActionResult<ApiResponse<ChannelDetailDto>>> GetChannelDetail(string channelName)
    {
        var channelDetail = await _pathLockService.GetChannelDetailAsync(channelName);

        if (channelDetail == null)
        {
            _logger.LogWarning("[PathLocksController] 通道 {ChannelName} 不存在或无活跃锁定", channelName);
            return NotFound(ApiResponse<ChannelDetailDto>.Fail("通道不存在或无活跃锁定"));
        }

        _logger.LogInformation("[PathLocksController] 获取通道详情成功: {ChannelName}, 锁定数={Count}",
            channelName, channelDetail.PathLocks.Count);
        return Ok(ApiResponse<ChannelDetailDto>.Ok(channelDetail));
    }

    /// <summary>
    /// 手动释放指定通道(仅释放已取消任务的锁定)
    /// </summary>
    /// <param name="channelName">通道名称</param>
    [HttpPost("channel/{channelName}/release")]
    public async Task<ActionResult<ApiResponse<int>>> ReleaseChannel(string channelName)
    {
        var releasedCount = await _pathLockService.ReleaseChannelAsync(channelName);

        _logger.LogInformation("[PathLocksController] 手动释放通道 {ChannelName} 完成: 释放数={Count}",
            channelName, releasedCount);

        return Ok(ApiResponse<int>.Ok(releasedCount));
    }
}
