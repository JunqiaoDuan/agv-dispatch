using AgvDispatch.Shared.DTOs.MapEdges;
using AgvDispatch.Shared.DTOs.MapNodes;
using AgvDispatch.Shared.DTOs.Maps;

namespace AgvDispatch.Web.Services;

/// <summary>
/// 地图 API 客户端接口
/// </summary>
public interface IMapClient
{
    #region 地图

    /// <summary>
    /// 获取所有地图列表
    /// </summary>
    Task<List<MapListItemDto>> GetAllAsync();

    /// <summary>
    /// 获取地图详情
    /// </summary>
    Task<MapDetailDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取下一个可用的地图编号
    /// </summary>
    Task<string?> GetNextCodeAsync();

    /// <summary>
    /// 创建地图
    /// </summary>
    Task<string?> CreateAsync(CreateMapRequest request);

    /// <summary>
    /// 更新地图
    /// </summary>
    Task<string?> UpdateAsync(Guid id, UpdateMapRequest request);

    /// <summary>
    /// 删除地图
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    #endregion

    #region 节点

    /// <summary>
    /// 获取地图的所有节点
    /// </summary>
    Task<List<MapNodeListItemDto>> GetNodesAsync(Guid mapId);

    /// <summary>
    /// 获取下一个可用的节点编号
    /// </summary>
    Task<string?> GetNextNodeCodeAsync(Guid mapId);

    /// <summary>
    /// 创建节点
    /// </summary>
    Task<string?> CreateNodeAsync(Guid mapId, CreateMapNodeRequest request);

    /// <summary>
    /// 更新节点
    /// </summary>
    Task<string?> UpdateNodeAsync(Guid mapId, Guid id, UpdateMapNodeRequest request);

    /// <summary>
    /// 删除节点
    /// </summary>
    Task<bool> DeleteNodeAsync(Guid mapId, Guid id);

    #endregion

    #region 边

    /// <summary>
    /// 获取地图的所有边
    /// </summary>
    Task<List<MapEdgeListItemDto>> GetEdgesAsync(Guid mapId);

    /// <summary>
    /// 获取下一个可用的边编号
    /// </summary>
    Task<string?> GetNextEdgeCodeAsync(Guid mapId);

    /// <summary>
    /// 创建边
    /// </summary>
    Task<string?> CreateEdgeAsync(Guid mapId, CreateMapEdgeRequest request);

    /// <summary>
    /// 更新边
    /// </summary>
    Task<string?> UpdateEdgeAsync(Guid mapId, Guid id, UpdateMapEdgeRequest request);

    /// <summary>
    /// 删除边
    /// </summary>
    Task<bool> DeleteEdgeAsync(Guid mapId, Guid id);

    #endregion
}
