using System.Linq.Expressions;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class ProductRepo : RepositoryBase<Product>, IProductRepo
    {
        private readonly IFileStorageService? _fileStorageService;

        public ProductRepo(WarehousingContext context, ILogger<ProductRepo> logger, IConfiguration config, IFileStorageService? fileStorageService = null) 
            : base(context, logger, config) 
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<Product> AddProduct(ProductDto dto)
        {
            try
            {
                Product product = new Product();
                if (dto.Image != null)
                {
                    var guid = Guid.NewGuid().ToString();
                    var fileName = $"{guid}_{dto.Image.FileName}";
                    
                    if (_fileStorageService != null)
                    {
                        // Use FileStorageService (Azure or Local)
                        using (var stream = dto.Image.OpenReadStream())
                        {
                            product.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
                        }
                    }
                    else
                    {
                        // Fallback to local storage (legacy code)
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Product");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string fileNameWithPath = Path.Combine(path, fileName);
                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            await dto.Image.CopyToAsync(stream);
                        }
                        product.ImagePath = Path.Combine("Resources", "Images", "Product", fileName).Replace("\\", "/");
                    }
                }

                // Get the last product ID, defaulting to 0 if no products exist
                // Materialize the query first since DefaultIfEmpty can't be translated by EF Core
                var allProductIds = await GetAll().Select(p => p.Id).ToListAsync();
                var lastProductId = allProductIds.Count > 0 ? allProductIds.Max() : 0;
                // Create product entity
                product.Code = (lastProductId + 1).ToString();
                product.NameEn = dto.NameEn != null ? dto.NameEn : dto.NameAr;
                product.NameAr = dto.NameAr;
                product.Description = dto.Description!;
                product.SubCategoryId = dto.SubCategoryId;
                product.UnitId = dto.UnitId;
                product.CostPrice = dto.CostPrice;
                product.SellingPrice = dto.SellingPrice;
                product.ReorderLevel = dto.ReorderLevel;

                var createdProduct = await CreateAsync(product);

                return createdProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product: {ProductCode}", dto?.Code);
                throw; // Rethrow to let controller handle it if needed
            }
        }

        public async Task<Product> UpdateProduct(ProductDto dto)
        {
            try
            {
                Product product = GetByCondition(x => x.Id == dto.Id).FirstOrDefault();
                if (product != null)
                {
                    if (dto.Image != null)
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(product.ImagePath))
                        {
                            if (_fileStorageService != null)
                            {
                                await _fileStorageService.DeleteFileAsync(product.ImagePath);
                            }
                            else
                            {
                                // Fallback to local delete
                                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), product.ImagePath);
                                if (File.Exists(imagePath))
                                {
                                    File.Delete(imagePath);
                                }
                            }
                        }

                        // Upload new image
                        var guid = Guid.NewGuid().ToString();
                        var fileName = $"{guid}_{dto.Image.FileName}";
                        
                        if (_fileStorageService != null)
                        {
                            // Use FileStorageService (Azure or Local)
                            using (var stream = dto.Image.OpenReadStream())
                            {
                                product.ImagePath = await _fileStorageService.SaveFileAsync(stream, fileName, "Product");
                            }
                        }
                        else
                        {
                            // Fallback to local storage (legacy code)
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Product");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            string fileNameWithPath = Path.Combine(path, fileName);
                            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                            {
                                await dto.Image.CopyToAsync(stream);
                            }
                            product.ImagePath = Path.Combine("Resources", "Images", "Product", fileName).Replace("\\", "/");
                        }
                    }

                    product.Code = dto.Code;
                    product.NameEn = dto.NameEn;
                    product.NameAr = dto.NameAr;
                    product.Description = dto.Description!;
                    product.IsActive = dto.IsActive;
                    product.SubCategoryId = dto.SubCategoryId;
                    product.UnitId = dto.UnitId;
                    product.CostPrice = dto.CostPrice;
                    product.SellingPrice = dto.SellingPrice;
                    
                    // Products are global - inventory is managed separately through Inventory table
                    // No need to handle inventory during product updates

                    var updatedProduct = await UpdateAsync(product);

                    return updatedProduct;
                }
                else
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", dto?.Id);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Update product: {ProductCode}", dto?.Code);
                throw; // Rethrow to let controller handle it if needed
            }
        }

        public async Task<IList<TResult>> GetAllPaginationWithProjection<TResult>(
            int pageIndex,
            int pageSize,
            Expression<Func<Product, int>> orderBy,
            Expression<Func<Product, bool>>? filter,
            Expression<Func<Product, TResult>> projection,
            params Expression<Func<Product, object>>[] includes
        )
        {
            // Start with the base query
            IQueryable<Product> query = _context.Set<Product>().AsNoTracking();

            // Apply filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply includes (related data)
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Apply ordering
            query = query.OrderBy(orderBy);

            // Apply pagination
            var paginatedQuery = query
                .Skip(pageSize * (pageIndex - 1))
                .Take(pageSize);

            // Apply projection and return result
            return await paginatedQuery
                .Select(projection)
                .ToListAsync();
        }
    }
}