using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.BackgroundJobLogs;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 后台任务日志API客户端接口
/// </summary>
public interface IBackgroundJobLogClient
{
    /// <summary>
    /// 按时间范围分页查询后台任务日志
    /// </summary>
    /// <param name="startTime">起始时间 (格式: yyyyMMdd)</param>
    /// <param name="endTime">结束时间 (格式: yyyyMMdd)</param>
    Task<BackgroundJobLogPagedResult?> GetByTimeRangeAsync(
        string? startTime = null,
        string? endTime = null,
        string? jobName = null,
        int? result = null,
        int pageIndex = 0,
        int pageSize = 50);

    /// <summary>
    /// 获取任务统计信息
    /// </summary>
    /// <param name="since">起始日期 (格式: yyyyMMdd)</param>
    Task<BackgroundJobStatistics?> GetStatisticsAsync(string? since = null);
}

/// <summary>
/// 后台任务日志API客户端实现
/// </summary>
public class BackgroundJobLogClient : IBackgroundJobLogClient
{
    private readonly HttpClient _http;

    public BackgroundJobLogClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<BackgroundJobLogPagedResult?> GetByTimeRangeAsync(
        string? startTime = null,
        string? endTime = null,
        string? jobName = null,
        int? result = null,
        int pageIndex = 0,
        int pageSize = 50)
    {
        var query = new List<string>();
        query.Add($"pageIndex={pageIndex}");
        query.Add($"pageSize={pageSize}");

        if (!string.IsNullOrWhiteSpace(startTime))
            query.Add($"startTime={startTime}");

        if (!string.IsNullOrWhiteSpace(endTime))
            query.Add($"endTime={endTime}");

        if (!string.IsNullOrWhiteSpace(jobName))
            query.Add($"jobName={Uri.EscapeDataString(jobName)}");

        if (result.HasValue)
            query.Add($"result={result.Value}");

        var queryString = string.Join("&", query);
        var response = await _http.GetFromJsonAsync<ApiResponse<BackgroundJobLogPagedResult>>($"api/backgroundjoblogs/range?{queryString}");

        return response?.Success == true ? response.Data : null;
    }

    public async Task<BackgroundJobStatistics?> GetStatisticsAsync(string? since = null)
    {
        var query = !string.IsNullOrWhiteSpace(since) ? $"?since={since}" : "";
        var response = await _http.GetFromJsonAsync<ApiResponse<BackgroundJobStatistics>>($"api/backgroundjoblogs/statistics{query}");

        return response?.Success == true ? response.Data : null;
    }
}
