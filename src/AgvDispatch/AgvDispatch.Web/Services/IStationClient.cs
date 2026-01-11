using AgvDispatch.Shared.DTOs.Stations;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 站点 API 客户端接口
/// </summary>
public interface IStationClient
{
    /// <summary>
    /// 获取地图的所有站点
    /// </summary>
    Task<List<StationListItemDto>> GetAllAsync(Guid mapId);

    /// <summary>
    /// 获取站点详情
    /// </summary>
    Task<StationListItemDto?> GetByIdAsync(Guid mapId, Guid id);

    /// <summary>
    /// 获取下一个可用的站点编号
    /// </summary>
    Task<string?> GetNextCodeAsync(Guid mapId);

    /// <summary>
    /// 创建站点
    /// </summary>
    Task<string?> CreateAsync(Guid mapId, CreateStationRequest request);

    /// <summary>
    /// 更新站点
    /// </summary>
    Task<string?> UpdateAsync(Guid mapId, Guid id, UpdateStationRequest request);

    /// <summary>
    /// 删除站点
    /// </summary>
    Task<bool> DeleteAsync(Guid mapId, Guid id);
}
