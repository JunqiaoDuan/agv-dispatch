using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
using AgvDispatch.Shared.DTOs.Stations;
using AgvDispatch.Shared.DTOs.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// AGV API 服务实现
/// </summary>
public class AgvApiService : IAgvApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;

    public AgvApiService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    private HttpClient GetHttpClient()
    {
        var client = _httpClientFactory.CreateClient("AgvApi");

        // 添加认证令牌
        var token = _authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    // AGV相关
    public async Task<List<AgvListItemDto>> GetAllAgvsAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvListItemDto>>>("api/agvs");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvListItemDto>();
        }
        catch
        {
            return new List<AgvListItemDto>();
        }
    }

    public async Task<AgvDetailDto?> GetAgvByIdAsync(Guid id)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<AgvDetailDto>>($"api/agvs/{id}");
            return response?.Success == true ? response.Data : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AgvMonitorItemDto>> GetAgvMonitorListAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvMonitorItemDto>>>("api/agvs/monitor");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvMonitorItemDto>();
        }
        catch
        {
            return new List<AgvMonitorItemDto>();
        }
    }

    public async Task<List<AgvExceptionSummaryDto>> GetAgvUnresolvedExceptionsAsync(string agvCode)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvExceptionSummaryDto>>>($"api/agvs/{agvCode}/exceptions/unresolved");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvExceptionSummaryDto>();
        }
        catch
        {
            return new List<AgvExceptionSummaryDto>();
        }
    }

    public async Task<PagedResponse<AgvExceptionSummaryDto>> GetAllAgvExceptionsAsync(string agvCode, PagedAgvExceptionRequest request)
    {
        try
        {
            var client = GetHttpClient();
            var queryString = $"?PageIndex={request.PageIndex}&PageSize={request.PageSize}&SortBy={request.SortBy}&SortDescending={request.SortDescending}";
            var response = await client.GetFromJsonAsync<ApiResponse<PagedResponse<AgvExceptionSummaryDto>>>($"api/agvs/{agvCode}/exceptions{queryString}");
            return response?.Success == true && response.Data != null ? response.Data : new PagedResponse<AgvExceptionSummaryDto>();
        }
        catch
        {
            return new PagedResponse<AgvExceptionSummaryDto>();
        }
    }

    public async Task<bool> ResolveExceptionsAsync(List<Guid> exceptionIds)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.PostAsJsonAsync("api/agvs/exceptions/resolve", exceptionIds);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result?.Success == true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    // 站点相关
    public async Task<List<StationListItemDto>> GetAllStationsAsync(Guid mapId)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<StationListItemDto>>>($"api/stations?mapId={mapId}");
            return response?.Success == true && response.Data != null ? response.Data : new List<StationListItemDto>();
        }
        catch
        {
            return new List<StationListItemDto>();
        }
    }

    // 任务相关
    public async Task<List<TaskListItemDto>> GetAllTasksAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<TaskListItemDto>>>("api/tasks");
            return response?.Success == true && response.Data != null ? response.Data : new List<TaskListItemDto>();
        }
        catch
        {
            return new List<TaskListItemDto>();
        }
    }

    public async Task<List<TaskListItemDto>> GetActiveTasksAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<TaskListItemDto>>>("api/tasks/active");
            return response?.Success == true && response.Data != null ? response.Data : new List<TaskListItemDto>();
        }
        catch
        {
            return new List<TaskListItemDto>();
        }
    }

    public async Task<List<AgvRecommendationDto>> GetRecommendationsAsync(GetRecommendationsRequestDto request)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.PostAsJsonAsync("api/tasks/recommendations", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<AgvRecommendationDto>>>();
                return result?.Success == true && result.Data != null ? result.Data : new List<AgvRecommendationDto>();
            }
            return new List<AgvRecommendationDto>();
        }
        catch
        {
            return new List<AgvRecommendationDto>();
        }
    }

    public async Task<CreateTaskResponseDto?> CreateTaskAsync(CreateTaskWithAgvRequestDto request)
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.PostAsJsonAsync("api/tasks", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<CreateTaskResponseDto>>();
                return result?.Success == true ? result.Data : null;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<AgvPendingItemDto>> GetPendingUnloadingAgvsAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/pending-unloading-agvs");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvPendingItemDto>();
        }
        catch
        {
            return new List<AgvPendingItemDto>();
        }
    }

    public async Task<List<AgvPendingItemDto>> GetPendingReturnAgvsAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/pending-return-agvs");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvPendingItemDto>();
        }
        catch
        {
            return new List<AgvPendingItemDto>();
        }
    }

    public async Task<List<AgvPendingItemDto>> GetChargeableAgvsAsync()
    {
        try
        {
            var client = GetHttpClient();
            var response = await client.GetFromJsonAsync<ApiResponse<List<AgvPendingItemDto>>>("api/tasks/chargeable-agvs");
            return response?.Success == true && response.Data != null ? response.Data : new List<AgvPendingItemDto>();
        }
        catch
        {
            return new List<AgvPendingItemDto>();
        }
    }
}
