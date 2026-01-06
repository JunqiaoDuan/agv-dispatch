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
}
