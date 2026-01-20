using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;

namespace AgvDispatch.Web.Services;

/// <summary>
/// AGV API 客户端实现
/// </summary>
public class AgvClient : IAgvClient
{
    private readonly HttpClient _http;

    public AgvClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AgvListItemDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvListItemDto>>>("api/agvs");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<List<AgvMonitorItemDto>> GetMonitorListAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvMonitorItemDto>>>("api/agvs/monitor");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<AgvDetailDto?> GetByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AgvDetailDto>>($"api/agvs/{id}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> GetNextCodeAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>("api/agvs/next-code");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateAsync(CreateAgvRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/agvs", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateAsync(Guid id, UpdateAgvRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/agvs/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/agvs/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AgvExceptionSummaryDto>> GetAgvUnresolvedExceptionsAsync(string agvCode)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvExceptionSummaryDto>>>($"api/agvs/{agvCode}/exceptions/unresolved");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<PagedResponse<AgvExceptionSummaryDto>> GetAllAgvExceptionsAsync(string agvCode, PagedAgvExceptionRequest request)
    {
        var queryParams = new List<string>();
        queryParams.Add($"PageIndex={request.PageIndex}");
        queryParams.Add($"PageSize={request.PageSize}");

        if (request.OnlyUnresolved.HasValue)
            queryParams.Add($"OnlyUnresolved={request.OnlyUnresolved.Value}");

        if (request.Severity.HasValue)
            queryParams.Add($"Severity={(int)request.Severity.Value}");

        if (!string.IsNullOrEmpty(request.SortBy))
            queryParams.Add($"SortBy={request.SortBy}");

        queryParams.Add($"SortDescending={request.SortDescending}");

        var queryString = string.Join("&", queryParams);
        var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<AgvExceptionSummaryDto>>>($"api/agvs/{agvCode}/exceptions/all?{queryString}");

        return response?.Success == true && response.Data != null ? response.Data : new PagedResponse<AgvExceptionSummaryDto>();
    }

    public async Task<bool> ResolveExceptionsAsync(List<Guid> exceptionIds)
    {
        var response = await _http.PostAsJsonAsync("api/agvs/exceptions/resolve", exceptionIds);
        return response.IsSuccessStatusCode;
    }
}
