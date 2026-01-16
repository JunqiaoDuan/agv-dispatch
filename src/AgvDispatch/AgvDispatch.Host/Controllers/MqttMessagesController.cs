using AgvDispatch.Business.Entities.MqttMessageLogAggregate;
using AgvDispatch.Infrastructure.Db.EF;
using AgvDispatch.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MqttMessagesController : ControllerBase
{
    private readonly AgvDispatchContext _dbContext;
    private readonly ILogger<MqttMessagesController> _logger;

    public MqttMessagesController(
        AgvDispatchContext dbContext,
        ILogger<MqttMessagesController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 按时间范围查询MQTT消息
    /// </summary>
    /// <param name="startTime">开始时间 (格式: yyyyMMdd)</param>
    /// <param name="endTime">结束时间 (格式: yyyyMMdd)</param>
    /// <param name="agvCode">AGV编号过滤</param>
    /// <param name="direction">消息方向（1=入站, 2=出站）</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="pageIndex">页码（从0开始）</param>
    /// <param name="pageSize">每页数量（默认50，最大200）</param>
    [HttpGet("range")]
    public async Task<ActionResult<ApiResponse<PagedResponse<MqttMessageLog>>>> GetByTimeRange(
        [FromQuery] string? startTime = null,
        [FromQuery] string? endTime = null,
        [FromQuery] string? agvCode = null,
        [FromQuery] int? direction = null,
        [FromQuery] string? messageType = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // 限制分页大小
            pageSize = Math.Min(pageSize, 200);

            var query = _dbContext.MqttMessageLogs
                .Where(m => m.IsValid)
                .AsQueryable();

            // 解析并过滤起始时间
            if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParseExact(startTime, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var start))
            {
                var startTimeOffset = new DateTimeOffset(start, TimeSpan.Zero);
                query = query.Where(m => m.Timestamp >= startTimeOffset);
            }

            // 解析并过滤结束时间
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParseExact(endTime, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var end))
            {
                var endTimeOffset = new DateTimeOffset(end.AddDays(1).AddSeconds(-1), TimeSpan.Zero);
                query = query.Where(m => m.Timestamp <= endTimeOffset);
            }

            // 其他过滤条件
            if (!string.IsNullOrWhiteSpace(agvCode))
                query = query.Where(m => m.AgvCode == agvCode);

            if (direction.HasValue)
                query = query.Where(m => m.Direction == (MqttMessageDirection)direction.Value);

            if (!string.IsNullOrWhiteSpace(messageType))
                query = query.Where(m => m.MessageType == messageType);

            // 获取总数
            var totalCount = await query.CountAsync();

            // 分页查询
            var messages = await query
                .OrderByDescending(m => m.Timestamp)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<MqttMessageLog>
            {
                Items = messages,
                TotalCount = totalCount
            };

            return Ok(ApiResponse<PagedResponse<MqttMessageLog>>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按时间范围查询MQTT消息失败");
            return Ok(ApiResponse<PagedResponse<MqttMessageLog>>.Fail($"查询消息失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取消息统计信息
    /// </summary>
    /// <param name="since">起始日期 (格式: yyyyMMdd)</param>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatistics(
        [FromQuery] string? since = null)
    {
        try
        {
            var query = _dbContext.MqttMessageLogs.Where(m => m.IsValid);

            // 解析并过滤起始时间
            if (!string.IsNullOrWhiteSpace(since) && DateTime.TryParseExact(since, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var sinceDate))
            {
                var sinceTimeOffset = new DateTimeOffset(sinceDate, TimeSpan.Zero);
                query = query.Where(m => m.Timestamp >= sinceTimeOffset);
            }

            // 分别执行各个统计查询
            var totalCount = await query.CountAsync();
            var inboundCount = await query.Where(m => m.Direction == MqttMessageDirection.Inbound).CountAsync();
            var outboundCount = await query.Where(m => m.Direction == MqttMessageDirection.Outbound).CountAsync();

            var byAgv = await query
                .GroupBy(m => m.AgvCode)
                .Select(g => new { AgvCode = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var byMessageType = await query
                .GroupBy(m => m.MessageType)
                .Select(g => new { MessageType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var statistics = new
            {
                TotalCount = totalCount,
                InboundCount = inboundCount,
                OutboundCount = outboundCount,
                ByAgv = byAgv,
                ByMessageType = byMessageType
            };

            return Ok(ApiResponse<object>.Ok(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取MQTT消息统计失败");
            return Ok(ApiResponse<object>.Fail($"获取统计失败: {ex.Message}"));
        }
    }

}
