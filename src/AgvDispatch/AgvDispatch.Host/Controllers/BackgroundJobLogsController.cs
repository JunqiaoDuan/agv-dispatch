using AgvDispatch.Business.Entities.BackgroundJobLogAggregate;
using AgvDispatch.Business.Specifications.BackgroundJobLogs;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.BackgroundJobLogs;
using AgvDispatch.Shared.Enums;
using AgvDispatch.Shared.Extensions;
using AgvDispatch.Shared.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgvDispatch.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class BackgroundJobLogsController : ControllerBase
{
    private readonly IRepository<BackgroundJobLog> _repository;
    private readonly ILogger<BackgroundJobLogsController> _logger;

    public BackgroundJobLogsController(
        IRepository<BackgroundJobLog> repository,
        ILogger<BackgroundJobLogsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// 按时间范围查询后台任务日志
    /// </summary>
    /// <param name="startTime">开始时间 (格式: yyyyMMdd)</param>
    /// <param name="endTime">结束时间 (格式: yyyyMMdd)</param>
    /// <param name="jobName">任务名称过滤</param>
    /// <param name="result">执行结果（1=Success, 2=Failed, 3=Skipped）</param>
    /// <param name="pageIndex">页码（从0开始）</param>
    /// <param name="pageSize">每页数量（默认50，最大200）</param>
    [HttpGet("range")]
    public async Task<ActionResult<ApiResponse<BackgroundJobLogPagedResult>>> GetByTimeRange(
        [FromQuery] string? startTime = null,
        [FromQuery] string? endTime = null,
        [FromQuery] string? jobName = null,
        [FromQuery] int? result = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 50)
    {
        // 限制分页大小
        pageSize = Math.Min(pageSize, 200);

        // 解析日期参数
        DateTime? startDate = null;
        if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParseExact(startTime, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var start))
        {
            startDate = start;
        }

        DateTime? endDate = null;
        if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParseExact(endTime, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var end))
        {
            endDate = end;
        }

        // 使用规约查询总数
        var countSpec = new BackgroundJobLogCountSpec(startDate, endDate, jobName, result);
        var totalCount = await _repository.CountAsync(countSpec);

        // 使用规约分页查询
        var pagedSpec = new BackgroundJobLogPagedSpec(startDate, endDate, jobName, result, pageIndex, pageSize);
        var logs = await _repository.ListAsync(pagedSpec);

        // 映射到 DTO
        var dtoList = logs.MapToList<BackgroundJobLogDto>();

        var response = new BackgroundJobLogPagedResult
        {
            Items = dtoList,
            TotalCount = totalCount
        };

        return Ok(ApiResponse<BackgroundJobLogPagedResult>.Ok(response));
    }

    /// <summary>
    /// 获取后台任务统计信息
    /// </summary>
    /// <param name="since">起始日期 (格式: yyyyMMdd)</param>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<BackgroundJobStatistics>>> GetStatistics(
        [FromQuery] string? since = null)
    {
        // 解析日期参数
        DateTime? sinceDate = null;
        if (!string.IsNullOrWhiteSpace(since) && DateTime.TryParseExact(since, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            sinceDate = parsedDate;
        }

        // 使用规约统计总数
        var statisticsSpec = new BackgroundJobLogStatisticsSpec(sinceDate);
        var totalCount = await _repository.CountAsync(statisticsSpec);

        // 使用规约统计各种结果类型的数量
        var successSpec = new BackgroundJobLogByResultSpec(sinceDate, JobExecutionResult.Success);
        var successCount = await _repository.CountAsync(successSpec);

        var failedSpec = new BackgroundJobLogByResultSpec(sinceDate, JobExecutionResult.Failed);
        var failedCount = await _repository.CountAsync(failedSpec);

        var skippedSpec = new BackgroundJobLogByResultSpec(sinceDate, JobExecutionResult.Skipped);
        var skippedCount = await _repository.CountAsync(skippedSpec);

        // 获取所有日志并在内存中进行 GroupBy 统计
        var allLogs = await _repository.ListAsync(statisticsSpec);
        var byJobType = allLogs
            .GroupBy(m => new { m.JobName, m.JobDisplayName })
            .Select(g => new JobTypeCount
            {
                JobName = g.Key.JobName,
                JobDisplayName = g.Key.JobDisplayName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var statistics = new BackgroundJobStatistics
        {
            TotalCount = totalCount,
            SuccessCount = successCount,
            FailedCount = failedCount,
            SkippedCount = skippedCount,
            ByJobType = byJobType
        };

        return Ok(ApiResponse<BackgroundJobStatistics>.Ok(statistics));
    }
}
