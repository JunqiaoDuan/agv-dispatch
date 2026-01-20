using AgvDispatch.Shared.Repository;
using Ardalis.Specification.EntityFrameworkCore;

namespace AgvDispatch.Infrastructure.Db.EF;

public class EFRepository<T> : RepositoryBase<T>, IRepository<T>, IReadRepository<T>
    where T : class, IAggregateRoot
{
    public EFRepository(AgvDispatchContext dbContext) : base(dbContext)
    {
    }
}
