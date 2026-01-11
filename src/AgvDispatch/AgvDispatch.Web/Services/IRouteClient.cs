using AgvDispatch.Shared.DTOs.Routes;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 路线 API 客户端接口
/// </summary>
public interface IRouteClient
{
    /// <summary>
    /// 获取指定地图的路线列表
    /// </summary>
    Task<List<RouteListItemDto>> GetAllAsync(Guid mapId);

    /// <summary>
    /// 获取路线详情
    /// </summary>
    Task<RouteDetailDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取下一个可用的路线编号
    /// </summary>
    Task<string?> GetNextCodeAsync(Guid mapId);

    /// <summary>
    /// 创建路线
    /// </summary>
    Task<string?> CreateAsync(CreateRouteRequest request);

    /// <summary>
    /// 更新路线
    /// </summary>
    Task<string?> UpdateAsync(Guid id, UpdateRouteRequest request);

    /// <summary>
    /// 删除路线
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
