using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 获取可以接收任务的 AGV（在线）
/// </summary>
public class AgvAvailableForTaskSpec : Specification<Agv>
{
    public AgvAvailableForTaskSpec()
    {
        Query.Where(x => x.IsValid
                      && x.AgvStatus == AgvStatus.Online)
            .OrderBy(x => x.SortNo);
    }
}
