using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Stations;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 站点 API 客户端实现
/// </summary>
public class StationClient : IStationClient
{
    private readonly HttpClient _http;

    public StationClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<StationListItemDto>> GetAllAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<StationListItemDto>>>($"api/maps/{mapId}/stations");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<StationListItemDto?> GetByIdAsync(Guid mapId, Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<StationListItemDto>>($"api/maps/{mapId}/stations/{id}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> GetNextCodeAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>($"api/maps/{mapId}/stations/next-code");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateAsync(Guid mapId, CreateStationRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/maps/{mapId}/stations", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateAsync(Guid mapId, Guid id, UpdateStationRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/maps/{mapId}/stations/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteAsync(Guid mapId, Guid id)
    {
        var response = await _http.DeleteAsync($"api/maps/{mapId}/stations/{id}");
        return response.IsSuccessStatusCode;
    }
}
