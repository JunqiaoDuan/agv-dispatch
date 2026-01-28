using System.Net.Http.Json;
using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.PathLocks;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 路径锁定 API 客户端实现
/// </summary>
public class PathLockClient : IPathLockClient
{
    private readonly HttpClient _http;

    public PathLockClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ActiveChannelDto>> GetActiveChannelsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ActiveChannelDto>>>("api/pathlocks/active-channels");
        return response?.Success == true && response.Data != null ? response.Data : [];
    }

    public async Task<ChannelDetailDto?> GetChannelDetailAsync(string channelName)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ChannelDetailDto>>($"api/pathlocks/channel/{Uri.EscapeDataString(channelName)}");
        return response?.Success == true ? response.Data : null;
    }

    public async Task<int> ReleaseChannelAsync(string channelName)
    {
        var response = await _http.PostAsJsonAsync($"api/pathlocks/channel/{Uri.EscapeDataString(channelName)}/release", new { });
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
        return result?.Success == true ? result.Data : 0;
    }
}
