using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Maps;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 地图 API 客户端实现
/// </summary>
public class MapClient : IMapClient
{
    private readonly HttpClient _http;

    public MapClient(HttpClient http)
    {
        _http = http;
    }

    #region 地图

    public async Task<List<MapListItemDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MapListItemDto>>>("api/maps");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<MapDetailDto?> GetByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<MapDetailDto>>($"api/maps/{id}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> GetNextCodeAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>("api/maps/next-code");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateAsync(CreateMapRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/maps", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateAsync(Guid id, UpdateMapRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/maps/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/maps/{id}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 节点

    public async Task<List<MapNodeListItemDto>> GetNodesAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MapNodeListItemDto>>>($"api/maps/{mapId}/nodes");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<string?> GetNextNodeCodeAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>($"api/maps/{mapId}/nodes/next-code");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateNodeAsync(Guid mapId, CreateMapNodeRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/maps/{mapId}/nodes", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateNodeAsync(Guid mapId, Guid id, UpdateMapNodeRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/maps/{mapId}/nodes/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteNodeAsync(Guid mapId, Guid id)
    {
        var response = await _http.DeleteAsync($"api/maps/{mapId}/nodes/{id}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region 边

    public async Task<List<MapEdgeListItemDto>> GetEdgesAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MapEdgeListItemDto>>>($"api/maps/{mapId}/edges");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<string?> GetNextEdgeCodeAsync(Guid mapId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<string>>($"api/maps/{mapId}/edges/next-code");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<string?> CreateEdgeAsync(Guid mapId, CreateMapEdgeRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/maps/{mapId}/edges", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "创建失败";
    }

    public async Task<string?> UpdateEdgeAsync(Guid mapId, Guid id, UpdateMapEdgeRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/maps/{mapId}/edges/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        return error?.Message ?? "更新失败";
    }

    public async Task<bool> DeleteEdgeAsync(Guid mapId, Guid id)
    {
        var response = await _http.DeleteAsync($"api/maps/{mapId}/edges/{id}");
        return response.IsSuccessStatusCode;
    }

    #endregion
}
