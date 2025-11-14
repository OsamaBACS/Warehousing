using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using Warehousing.Data.Entities;

namespace Warehousing.Repo.Interfaces
{
    public interface IProductRecipeRepo : IRepositoryBase<ProductRecipe>
    {
        Task<IEnumerable<ProductRecipeDto>> GetRecipeByParentProductAsync(int parentProductId);
        Task<IEnumerable<ProductRecipeDto>> GetRecipeByComponentProductAsync(int componentProductId);
        Task<bool> ValidateRecipeAsync(int parentProductId);
        Task<decimal> CalculateTotalCostAsync(int parentProductId);
    }
}
