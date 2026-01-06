using AgvDispatch.Shared.DTOs.Agvs;

namespace AgvDispatch.Web.Services;

/// <summary>
/// AGV API 客户端接口
/// </summary>
public interface IAgvClient
{
    /// <summary>
    /// 获取所有 AGV 列表
    /// </summary>
    Task<List<AgvListItemDto>> GetAllAsync();

    /// <summary>
    /// 获取单个 AGV 详情
    /// </summary>
    Task<AgvDetailDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取下一个可用的 AGV 编号
    /// </summary>
    Task<string?> GetNextCodeAsync();

    /// <summary>
    /// 创建 AGV
    /// </summary>
    /// <returns>成功返回 null，失败返回错误消息</returns>
    Task<string?> CreateAsync(CreateAgvRequest request);

    /// <summary>
    /// 更新 AGV
    /// </summary>
    /// <returns>成功返回 null，失败返回错误消息</returns>
    Task<string?> UpdateAsync(Guid id, UpdateAgvRequest request);

    /// <summary>
    /// 删除 AGV
    /// </summary>
    /// <returns>成功返回 true</returns>
    Task<bool> DeleteAsync(Guid id);
}
