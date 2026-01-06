using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Shared.Constants;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 获取V开头的最大编号的小车
/// </summary>
public class AgvMaxCodeSpec : Specification<Agv>, ISingleResultSpecification<Agv>
{
    public AgvMaxCodeSpec()
    {
        Query.Where(x => x.AgvCode.StartsWith(EntityCodes.AgvPrefix))
            .OrderByDescending(x => x.AgvCode);
    }
}
