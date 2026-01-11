using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Routes;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 路线 API 客户端实现
/// </summary>
public class RouteClient : IRouteClient
{
    private readonly HttpClient _http;

    public RouteClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RouteListItemDto>> GetAllAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<RouteListItemDto>>>($"api/routes?mapId={mapId}");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<RouteDetailDto?> GetByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<RouteDetailDto>>($"api/routes/{id}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> GetNextCodeAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>($"api/routes/next-code?mapId={mapId}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateAsync(CreateRouteRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/routes", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateAsync(Guid id, UpdateRouteRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/routes/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/routes/{id}");
        return response.IsSuccessStatusCode;
    }
}
