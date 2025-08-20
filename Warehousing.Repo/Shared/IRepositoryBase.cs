using System.Linq.Expressions;

namespace Warehousing.Repo.Shared
{
    public interface IRepositoryBase<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression);
        IQueryable<T> GetAllActive(Expression<Func<T, bool>> expression);
        IQueryable<T> GetByConditionIncluding(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        Task<T> CreateAsync(T entity);
        Task CreateRangeAsync(IList<T> entities);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRange(IList<T> entities);

        Task<IList<T>> GetAllPagination(int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, Expression<Func<T, bool>>? expression, params Expression<Func<T, object>>[]? includes);
        Task<IList<T>> Search(int pageIndex, int pageSize, Expression<Func<T, bool>> expression, Expression<Func<T, int>> orderBy, params Expression<Func<T, object>>[]? includes);
        Task<int> GetTotalCount();
    }
}