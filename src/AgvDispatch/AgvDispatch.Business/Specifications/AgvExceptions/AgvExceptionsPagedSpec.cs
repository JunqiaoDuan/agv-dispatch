using AgvDispatch.Business.Entities.AgvAggregate;
using AgvDispatch.Shared.Enums;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.AgvExceptions;

/// <summary>
/// 分页查询AGV异常列表
/// </summary>
public class AgvExceptionsPagedSpec : Specification<AgvExceptionLog>
{
    public AgvExceptionsPagedSpec(
        string agvCode,
        bool? onlyUnresolved,
        AgvExceptionSeverity? severity,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize)
    {
        // 基础过滤
        Query.Where(x => x.AgvCode == agvCode && x.IsValid);

        // 是否只查询未解决的
        if (onlyUnresolved.HasValue && onlyUnresolved.Value)
        {
            Query.Where(x => !x.IsResolved);
        }

        // 严重程度过滤
        if (severity.HasValue)
        {
            Query.Where(x => x.Severity == severity.Value);
        }

        // 排序
        switch (sortBy?.ToLower())
        {
            case "exceptiontime":
                if (sortDescending) Query.OrderByDescending(x => x.ExceptionTime);
                else Query.OrderBy(x => x.ExceptionTime);
                break;
            case "severity":
                if (sortDescending) Query.OrderByDescending(x => x.Severity);
                else Query.OrderBy(x => x.Severity);
                break;
            case "exceptiontype":
                if (sortDescending) Query.OrderByDescending(x => x.ExceptionType);
                else Query.OrderBy(x => x.ExceptionType);
                break;
            case "resolvedtime":
                if (sortDescending) Query.OrderByDescending(x => x.ResolvedTime);
                else Query.OrderBy(x => x.ResolvedTime);
                break;
            default:
                // 默认按异常时间倒序
                Query.OrderByDescending(x => x.ExceptionTime);
                break;
        }

        // 分页
        Query.Skip(pageIndex * pageSize).Take(pageSize);
    }
}

/// <summary>
/// 计数用规格书（不带分页）
/// </summary>
public class AgvExceptionsCountSpec : Specification<AgvExceptionLog>
{
    public AgvExceptionsCountSpec(string agvCode, bool? onlyUnresolved, AgvExceptionSeverity? severity)
    {
        // 基础过滤
        Query.Where(x => x.AgvCode == agvCode && x.IsValid);

        // 是否只查询未解决的
        if (onlyUnresolved.HasValue && onlyUnresolved.Value)
        {
            Query.Where(x => !x.IsResolved);
        }

        // 严重程度过滤
        if (severity.HasValue)
        {
            Query.Where(x => x.Severity == severity.Value);
        }
    }
}
