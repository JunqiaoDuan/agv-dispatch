using AgvDispatch.Shared.DTOs;
using AgvDispatch.Shared.DTOs.Agvs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgvDispatch.Mobile.Services;

/// <summary>
/// AGV API 服务接口
/// </summary>
public interface IAgvApiService
{
    Task<List<AgvListItemDto>> GetAllAgvsAsync();
    Task<AgvDetailDto?> GetAgvByIdAsync(Guid id);
}
