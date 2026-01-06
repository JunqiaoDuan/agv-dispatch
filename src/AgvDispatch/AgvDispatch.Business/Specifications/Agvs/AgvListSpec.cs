using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 获取有效的小车列表
/// </summary>
public class AgvListSpec : Specification<Agv>
{
    public AgvListSpec()
    {
        Query.Where(x => x.IsValid)
            .OrderBy(x => x.SortNo)
            .ThenBy(x => x.AgvCode);
    }
}
