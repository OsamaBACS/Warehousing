using System.Linq.Expressions;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Models;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Interfaces
{
    public interface IUserRepo : IRepositoryBase<User>
    {
        Task<LoginResult> Login(LoginDto user);
        Task<IList<User>> CustomeGetAllPagination(
        int pageIndex,
        int pageSize,
        Expression<Func<User, int>> orderBy,
        params Expression<Func<User, object>>[]? includes);

        Task<IList<User>> CustomeSearch(int pageIndex, int pageSize, Expression<Func<User, bool>> expression, Expression<Func<User, int>> orderBy, params Expression<Func<User, object>>[]? includes);
    }
}