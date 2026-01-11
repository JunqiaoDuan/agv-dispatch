using AgvDispatch.Business.Entities.StationAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Stations;

/// <summary>
/// 根据 ID 获取站点
/// </summary>
public class StationByIdSpec : Specification<Station>, ISingleResultSpecification<Station>
{
    public StationByIdSpec(Guid id)
    {
        Query.Where(x => x.Id == id && x.IsValid);
    }
}
