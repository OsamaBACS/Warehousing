using System.Linq.Expressions;
using Warehousing.Data.Context;
using Warehousing.Repo.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Shared
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        public readonly WarehousingContext _context;
        protected readonly ILogger _logger;
        private readonly IConfiguration _config;

        public RepositoryBase(WarehousingContext context, ILogger<RepositoryBase<T>> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public RepositoryBase(WarehousingContext context, ILogger<CategoryRepo> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public RepositoryBase(WarehousingContext context, ILogger<UnitRepo> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<T> CreateAsync(T entity)
        {
            _logger.LogInformation("Adding a new {Entity}", typeof(T).Name);
            var result = await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task CreateRangeAsync(IList<T> entities)
        {
            _logger.LogInformation("Adding a range of {Entity}", typeof(T).Name);
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _logger.LogInformation("Deleting {Entity}", typeof(T).Name);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRange(IList<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            _logger.LogInformation("Deleting {Entities}", typeof(T).Name);
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetAll()
        {
            _logger.LogInformation("Retrieving all records for {Entity}", typeof(T).Name);
            return _context.Set<T>().AsNoTracking();
        }

        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            _logger.LogInformation("Retrieving all records with condition for {Entity}", typeof(T).Name);
            return _context.Set<T>().Where(expression).AsNoTracking();
        }

        public IQueryable<T> GetAllActive(Expression<Func<T, bool>> expression)
        {
            _logger.LogInformation("Retrieving all active records for {Entity}", typeof(T).Name);
            return _context.Set<T>().Where(expression).AsNoTracking();
        }

        public IQueryable<T> GetByConditionIncluding(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            _logger.LogInformation("Retrieving all records with includ by condition for {Entity}", typeof(T).Name);
            var dbSet = _context.Set<T>().Where(expression).AsNoTracking();
            IQueryable<T> query = dbSet;
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return query;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _logger.LogInformation("Updating {Entity}", typeof(T).Name);
            var entry = _context.Entry(entity);
            var result = _context.Set<T>().Update(entity);
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IList<T>> GetAllPagination(
                int pageIndex,
                int pageSize,
                Expression<Func<T, object>> orderBy,
                Expression<Func<T, bool>>? expression = null,
                params Expression<Func<T, object>>[]? includes
            )
        {
            _logger.LogInformation("Retrieving paginated records for {Entity}", typeof(T).Name);

            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            // Apply filter if exists
            if (expression != null)
            {
                query = query.Where(expression);
            }

            // Apply includes if any
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply ordering, then pagination
            query = query.OrderByDescending(orderBy)
                         .Skip(pageSize * (pageIndex - 1))
                         .Take(pageSize);

            return await query.ToListAsync();
        }


        public async Task<IList<T>> Search(int pageIndex, int pageSize, Expression<Func<T, bool>> expression, Expression<Func<T, int>> orderBy, params Expression<Func<T, object>>[]? includes)
        {
            _logger.LogInformation("Retrieving all records by search using keyword and pagination for {Entity}", typeof(T).Name);
            var dbSet = _context.Set<T>().Where(expression).AsNoTracking();
            IQueryable<T> query = dbSet;
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);
                }
                ;
            }

            return await query.Skip(pageSize * (pageIndex - 1)).Take(pageSize).OrderBy(orderBy).ToListAsync();
        }

        public async Task<int> GetTotalCount()
        {
            return await _context.Set<T>().CountAsync();
        }
    }
}