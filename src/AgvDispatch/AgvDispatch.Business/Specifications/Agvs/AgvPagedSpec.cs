using AgvDispatch.Business.Entities.AgvAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Agvs;

/// <summary>
/// 分页查询小车列表
/// </summary>
public class AgvPagedSpec : Specification<Agv>
{
    public AgvPagedSpec(string? searchText, string? sortBy, bool sortDescending, int pageIndex, int pageSize)
    {
        Query.Where(x => x.IsValid);

        // 搜索过滤
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            Query.Where(x =>
                x.AgvCode.ToLower().Contains(search) ||
                x.DisplayName.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        // 排序
        switch (sortBy?.ToLower())
        {
            case "agvcode":
                if (sortDescending) Query.OrderByDescending(x => x.AgvCode);
                else Query.OrderBy(x => x.AgvCode);
                break;
            case "displayname":
                if (sortDescending) Query.OrderByDescending(x => x.DisplayName);
                else Query.OrderBy(x => x.DisplayName);
                break;
            case "agvstatus":
                if (sortDescending) Query.OrderByDescending(x => x.AgvStatus);
                else Query.OrderBy(x => x.AgvStatus);
                break;
            case "battery":
                if (sortDescending) Query.OrderByDescending(x => x.Battery);
                else Query.OrderBy(x => x.Battery);
                break;
            case "creationdate":
                if (sortDescending) Query.OrderByDescending(x => x.CreationDate);
                else Query.OrderBy(x => x.CreationDate);
                break;
            default:
                Query.OrderBy(x => x.SortNo).ThenBy(x => x.AgvCode);
                break;
        }

        // 分页
        Query.Skip(pageIndex * pageSize).Take(pageSize);
    }
}

/// <summary>
/// 计数用规格书（不带分页）
/// </summary>
public class AgvCountSpec : Specification<Agv>
{
    public AgvCountSpec(string? searchText)
    {
        Query.Where(x => x.IsValid);

        // 搜索过滤
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.ToLower();
            Query.Where(x =>
                x.AgvCode.ToLower().Contains(search) ||
                x.DisplayName.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }
    }
}
