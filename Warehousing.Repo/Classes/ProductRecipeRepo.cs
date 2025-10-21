using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class ProductRecipeRepo : RepositoryBase<ProductRecipe>, IProductRecipeRepo
    {
        private readonly WarehousingContext _context;
        private readonly IMapper _mapper;

        public ProductRecipeRepo(WarehousingContext context, ILogger<ProductRecipeRepo> logger, IConfiguration config) : base(context, logger, config)
        {
            _context = context;
            _mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()));
        }

        public async Task<IEnumerable<ProductRecipeDto>> GetRecipeByParentProductAsync(int parentProductId)
        {
            var recipes = await _context.ProductRecipes
                .Include(r => r.ParentProduct)
                .Include(r => r.ComponentProduct)
                .Where(r => r.ParentProductId == parentProductId && r.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductRecipeDto>>(recipes);
        }

        public async Task<IEnumerable<ProductRecipeDto>> GetRecipeByComponentProductAsync(int componentProductId)
        {
            var recipes = await _context.ProductRecipes
                .Include(r => r.ParentProduct)
                .Include(r => r.ComponentProduct)
                .Where(r => r.ComponentProductId == componentProductId && r.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductRecipeDto>>(recipes);
        }

        public async Task<bool> ValidateRecipeAsync(int parentProductId)
        {
            // Check if recipe has circular dependencies
            var recipes = await _context.ProductRecipes
                .Include(r => r.ComponentProduct)
                .Where(r => r.ParentProductId == parentProductId && r.IsActive)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                // Check if component is also a parent of the same product (circular dependency)
                var hasCircularDependency = await _context.ProductRecipes
                    .AnyAsync(r => r.ParentProductId == recipe.ComponentProductId && 
                                  r.ComponentProductId == parentProductId && 
                                  r.IsActive);

                if (hasCircularDependency)
                    return false;

                // Recursively check nested components
                if (!await ValidateRecipeAsync(recipe.ComponentProductId))
                    return false;
            }

            return true;
        }

        public async Task<decimal> CalculateTotalCostAsync(int parentProductId)
        {
            var recipes = await GetRecipeByParentProductAsync(parentProductId);
            decimal totalCost = 0;

            foreach (var recipe in recipes)
            {
                // Get component's cost price
                var component = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == recipe.ComponentProductId);

                if (component != null)
                {
                    // Calculate cost for this component
                    var componentCost = recipe.Quantity * component.CostPrice;
                    
                    // If component has its own recipe, calculate recursively
                    var hasSubRecipe = await _context.ProductRecipes
                        .AnyAsync(r => r.ParentProductId == recipe.ComponentProductId && r.IsActive);

                    if (hasSubRecipe)
                    {
                        componentCost = recipe.Quantity * await CalculateTotalCostAsync(recipe.ComponentProductId);
                    }

                    totalCost += componentCost;
                }
            }

            return totalCost;
        }
    }
}
