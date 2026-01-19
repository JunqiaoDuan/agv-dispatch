using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Business.Messages;
using AgvDispatch.Shared.Enums;

namespace AgvDispatch.Business.Services;

/// <summary>
/// 小车推荐服务接口
/// </summary>
public interface IAgvRecommendationService
{
    /// <summary>
    /// 获取上料推荐小车列表
    /// 筛选条件：Idle + Standby站点 + !HasCargo + Battery≥阈值
    /// 排序：站点优先级降序
    /// </summary>
    /// <param name="minBattery">最低电量要求(默认20%)</param>
    /// <param name="topCount">返回Top数量(默认10)</param>
    /// <returns>推荐列表(按评分降序)</returns>
    Task<List<AgvRecommendation>> GetLoadingRecommendationsAsync(
        int minBattery,
        int topCount);

    /// <summary>
    /// 获取等待下料的小车列表
    /// 筛选条件：Idle + Pickup站点 + !HasCargo
    /// </summary>
    /// <returns>等待下料的小车列表</returns>
    Task<List<AgvPendingItem>> GetPendingUnloadingAgvsAsync();

    /// <summary>
    /// 获取等待返回的小车列表
    /// 筛选条件：Idle + HasCargo（不限制站点类型）
    /// </summary>
    /// <returns>等待返回的小车列表</returns>
    Task<List<AgvPendingItem>> GetPendingReturnAgvsAsync();

    /// <summary>
    /// 获取可充电的小车列表
    /// 筛选条件：Idle
    /// 排序：电量升序（电量低的在前）
    /// </summary>
    /// <returns>可充电的小车列表</returns>
    Task<List<AgvPendingItem>> GetChargeableAgvsAsync();
}
