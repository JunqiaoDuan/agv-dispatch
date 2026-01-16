using System.Net.Http.Json;
using AgvDispatch.Business.Entities.MqttMessageLogAggregate;
using AgvDispatch.Shared.DTOs;

namespace AgvDispatch.Web.Services;

/// <summary>
/// MQTT消息API客户端实现
/// </summary>
public class MqttMessageClient : IMqttMessageClient
{
    private readonly HttpClient _http;

    public MqttMessageClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedResponse<MqttMessageLog>?> GetByTimeRangeAsync(
        string? startTime = null,
        string? endTime = null,
        string? agvCode = null,
        int? direction = null,
        string? messageType = null,
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

        if (!string.IsNullOrWhiteSpace(agvCode))
            query.Add($"agvCode={Uri.EscapeDataString(agvCode)}");

        if (direction.HasValue)
            query.Add($"direction={direction.Value}");

        if (!string.IsNullOrWhiteSpace(messageType))
            query.Add($"messageType={Uri.EscapeDataString(messageType)}");

        var queryString = string.Join("&", query);
        var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<MqttMessageLog>>>($"api/mqttmessages/range?{queryString}");

        return response?.Success == true ? response.Data : null;
    }

    public async Task<MqttMessageStatistics?> GetStatisticsAsync(string? since = null)
    {
        var query = !string.IsNullOrWhiteSpace(since) ? $"?since={since}" : "";
        var response = await _http.GetFromJsonAsync<ApiResponse<MqttMessageStatistics>>($"api/mqttmessages/statistics{query}");

        return response?.Success == true ? response.Data : null;
    }
}
