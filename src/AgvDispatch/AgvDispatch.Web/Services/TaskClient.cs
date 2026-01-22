using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Tasks;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 任务 API 客户端实现
/// </summary>
public class TaskClient : ITaskClient
{
    private readonly HttpClient _http;

    public TaskClient(HttpClient http)
    {
        _http = http;
    }

    #region AGV推荐/待处理

    public async Task<List<AgvRecommendationDto>> GetRecommendationsAsync(GetRecommendationsRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/tasks/recommendations", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<AgvRecommendationDto>>>();
            return result?.Data ?? [];
        }
        return [];
    }

    public async Task<List<AgvPendingItemDto>> GetPendingUnloadingAgvsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/pending-unloading-agvs");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<List<AgvPendingItemDto>> GetPendingReturnAgvsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/pending-return-agvs");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<List<AgvPendingItemDto>> GetChargeableAgvsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/chargeable-agvs");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    #endregion

    #region 任务CRUD

    public async Task<CreateTaskResponseDto?> CreateTaskAsync(CreateTaskWithAgvRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/tasks/create", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateTaskResponseDto>>();
            return result?.Data;
        }
        return null;
    }

    public async Task<string?> CancelTaskAsync(CancelTaskRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/tasks/cancel", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "取消失败";
    }

    public async Task<List<TaskListItemDto>> GetActiveTasksAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<TaskListItemDto>>>("api/tasks/active");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<TaskDetailDto?> GetByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<TaskDetailDto>>($"api/tasks/{id}");
        return response?.Success == true ? response.Data : null;
    }

    #endregion

}
