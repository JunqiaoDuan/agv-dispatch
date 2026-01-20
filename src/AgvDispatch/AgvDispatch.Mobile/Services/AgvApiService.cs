using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
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
}
