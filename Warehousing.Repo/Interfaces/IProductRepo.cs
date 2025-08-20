using System.Linq.Expressions;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Interfaces
{
    public interface IProductRepo : IRepositoryBase<Product>
    {
        Task<Product> AddProduct(ProductDto ProductDto);
        Task<Product> UpdateProduct(ProductDto ProductDto);
        Task<IList<TResult>> GetAllPaginationWithProjection<TResult>(
            int pageIndex,
            int pageSize,
            Expression<Func<Product, int>> orderBy,
            Expression<Func<Product, bool>> filter,
            Expression<Func<Product, TResult>> projection,
            params Expression<Func<Product, object>>[] includes
        );
    }
}