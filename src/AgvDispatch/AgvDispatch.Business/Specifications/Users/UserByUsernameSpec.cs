using AgvDispatch.Business.Entities.UserAggregate;
using Ardalis.Specification;

namespace AgvDispatch.Business.Specifications.Users;

/// <summary>
/// 根据用户名查询用户
/// </summary>
public class UserByUsernameSpec : Specification<User>, ISingleResultSpecification<User>
{
    public UserByUsernameSpec(string username)
    {
        Query.Where(x => x.IsValid && x.Username == username);
    }
}
