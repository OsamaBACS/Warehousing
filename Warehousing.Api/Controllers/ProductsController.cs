using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using Warehousing.Repo.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService? _fileStorageService;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService? fileStorageService = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        private string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            if (_fileStorageService != null)
            {
                return _fileStorageService.GetFileUrl(imagePath);
            }

            // Fallback: return relative path
            return $"/{imagePath.Replace("\\", "/")}";
        }

        [HttpGet]
        [Route("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var list = await _unitOfWork.ProductRepo
                    .GetAll()
                    .Include(c => c.SubCategory)
                    .Include(u => u.Unit)
                    .Include(u => u.Inventories).ThenInclude(s => s.Store)
                    .Include(p => p.Variants) // Include variants
                    .Include(p => p.ModifierGroups).ThenInclude(mg => mg.Modifier).ThenInclude(m => m.Options) // Include modifier groups with modifiers and options
                    .ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductsBySubCategoryId")]
        public async Task<IActionResult> GetProductsByCategoryId(int SubCategoryId)
        {
            try
            {
                var products = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.SubCategoryId == SubCategoryId && p.IsActive)
                    .Include(ps => ps.SubCategory)
                    .Include(ps => ps.Unit)
                    .Include(ps => ps.Inventories).ThenInclude(i => i.Store)
                    .Include(ps => ps.Variants)
                    .Include(ps => ps.ModifierGroups).ThenInclude(mg => mg.Modifier).ThenInclude(m => m.Options)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductById")]
        public async Task<IActionResult> GetProductById(int Id)
        {
            try
            {
                var product = await _unitOfWork.ProductRepo
                    .GetByCondition(u => u.Id == Id)
                    .Include(c => c.SubCategory)
                    .Include(u => u.Unit)
                    .Include(i => i.Inventories).ThenInclude(s => s.Store)
                    .Include(p => p.Variants).ThenInclude(v => v.Inventories)
                    .Include(p => p.ModifierGroups).ThenInclude(mg => mg.Modifier).ThenInclude(m => m.Options)
                    .FirstOrDefaultAsync();
                if (product == null)
                {
                    return NotFound("Product Not Found!");
                }
                else
                {
                    return Ok(product);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetProductsPagination")]
        public async Task<IActionResult> GetProductsPagination(int pageIndex, int pageSize)
        {
            try
            {
                var query = _unitOfWork.ProductRepo
                    .GetAll()
                    .Include(c => c.SubCategory)
                    .Include(u => u.Unit)
                    .Include(u => u.Inventories).ThenInclude(s => s.Store)
                    .Include(p => p.Variants);

                var products = await query
                    .OrderBy(p => p.Id)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .ToListAsync();

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    NameEn = p.NameEn,
                    NameAr = p.NameAr,
                    Description = p.Description ?? string.Empty,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    ImagePath = GetImageUrl(p.ImagePath),
                    IsActive = p.IsActive,
                    SubCategoryId = p.SubCategoryId,
                    SubCategory = p.SubCategory != null ? new SubCategoryDto
                    {
                        Id = p.SubCategory.Id,
                        NameEn = p.SubCategory.NameEn,
                        NameAr = p.SubCategory.NameAr,
                        Description = p.SubCategory.Description,
                        ImagePath = GetImageUrl(p.SubCategory.ImagePath),
                        IsActive = p.SubCategory.IsActive,
                        CategoryId = p.SubCategory.CategoryId
                        // No Products collection to prevent circular reference
                    } : null,
                    UnitId = p.UnitId,
                    Unit = _mapper.Map<UnitDto>(p.Unit),
                    Inventories = _mapper.Map<List<InventoryDto>>(p.Inventories),
                    Variants = p.Variants.Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Name = v.Name,
                        Code = v.Code,
                        Description = v.Description,
                        PriceAdjustment = v.PriceAdjustment,
                        CostAdjustment = v.CostAdjustment,
                        ReorderLevel = v.ReorderLevel,
                        IsActive = v.IsActive,
                        IsDefault = v.IsDefault,
                        DisplayOrder = v.DisplayOrder,
                        Inventories = _mapper.Map<List<InventoryDto>>(v.Inventories),
                        OrderItems = _mapper.Map<List<Warehousing.Repo.Dtos.OrderItemDto>>(v.OrderItems)
                        // No Product navigation property to prevent circular reference
                    }).ToList(),
                }).ToList();

                // Load variant stock data for products with variants
                foreach (var productDto in productDtos)
                {
                    if (productDto.Variants != null && productDto.Variants.Any())
                    {
                        var variantStockData = new Dictionary<string, object>();
                        
                        // Get all stores
                        var stores = await _unitOfWork.StoreRepo.GetAll().ToListAsync();
                        
                        foreach (var store in stores)
                        {
                            var stockData = await GetProductVariantsStockInternal(productDto.Id!.Value, store.Id);
                            variantStockData[$"store_{store.Id}"] = stockData;
                        }
                        
                        productDto.VariantStockData = variantStockData;
                    }
                }

                var totalSize = await _unitOfWork.ProductRepo.GetTotalCount();

                return Ok(new
                {
                    products = productDtos,
                    totals = totalSize
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet]
        [Route("SearchProductsPagination")]
        public async Task<IActionResult> SearchProductsPagination(int pageIndex, int pageSize, string keyword = "", int StoreId = 0)
        {
            try
            {
                var query = _unitOfWork.ProductRepo.GetAll(); // IQueryable<Product>

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.NameAr.Contains(keyword) || x.NameEn.Contains(keyword));
                }

                if (StoreId > 0)
                {
                    query = query.Where(x => x.Inventories.Any(s => s.StoreId == StoreId));
                }

                // Get total count *before* pagination
                var totalSize = await query.CountAsync();

                // Apply includes and projection
                var list = await query
                    .Include(c => c.SubCategory)
                    .Include(u => u.Unit)
                    .Include(u => u.Inventories).ThenInclude(s => s.Store)
                    .Include(p => p.Variants)
                    .OrderBy(x => x.Id)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Code = p.Code,
                        NameEn = p.NameEn,
                        NameAr = p.NameAr,
                        Description = p.Description ?? string.Empty,
                        CostPrice = p.CostPrice,
                        SellingPrice = p.SellingPrice,
                        ImagePath = GetImageUrl(p.ImagePath),
                        IsActive = p.IsActive,
                        SubCategoryId = p.SubCategoryId,
                        SubCategory = p.SubCategory != null ? new SubCategoryDto
                        {
                            Id = p.SubCategory.Id,
                            NameEn = p.SubCategory.NameEn,
                            NameAr = p.SubCategory.NameAr,
                            Description = p.SubCategory.Description,
                            ImagePath = GetImageUrl(p.SubCategory.ImagePath),
                            IsActive = p.SubCategory.IsActive,
                            CategoryId = p.SubCategory.CategoryId
                            // No Products collection to prevent circular reference
                        } : null,
                        UnitId = p.UnitId,
                        Unit = _mapper.Map<UnitDto>(p.Unit),
                        Inventories = _mapper.Map<List<InventoryDto>>(p.Inventories),
                        Variants = _mapper.Map<List<ProductVariantDto>>(p.Variants),
                    })
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    products = list,
                    totals = totalSize
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost, DisableRequestSizeLimit]
        [Route("SaveProduct")]
        public async Task<IActionResult> SaveProduct([FromForm] ProductDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(ModelState);

                // Products are global entities - no store-specific data needed

                // Additional validation for required fields
                if (string.IsNullOrEmpty(model.NameAr))
                {
                    ModelState.AddModelError("NameAr", "Product name in Arabic is required");
                }

                if (model.SubCategoryId == null || model.SubCategoryId <= 0)
                {
                    ModelState.AddModelError("SubCategoryId", "SubCategory is required");
                }

                if (model.UnitId == null || model.UnitId <= 0)
                {
                    ModelState.AddModelError("UnitId", "Unit is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (model.Id > 0)
                {
                    //Edit
                    var product = await _unitOfWork.ProductRepo.UpdateProduct(model);
                    return Ok(product);
                }
                else
                {
                    //Add
                    var product = await _unitOfWork.ProductRepo.AddProduct(model);
                    return Ok(product);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTotalCount")]
        public async Task<IActionResult> GetTotalCount()
        {
            try
            {
                var count = await _unitOfWork.ProductRepo.GetTotalCount();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(string searchTerm)
        {
            try
            {
                var products = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.NameAr.Contains(searchTerm) || 
                                         p.NameEn.Contains(searchTerm) || 
                                         p.Code.Contains(searchTerm))
                    .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.SubCategory.CategoryId == categoryId && p.IsActive)
                    .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-subcategory/{subCategoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsBySubCategory(int subCategoryId)
        {
            try
            {
                var products = await _unitOfWork.ProductRepo
                    .GetByCondition(p => p.SubCategoryId == subCategoryId && p.IsActive)
                    .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<object>>> GetLowStockProducts([FromQuery] decimal threshold = 10)
        {
            try
            {
                var lowStockProducts = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.Quantity <= threshold)
                    .Select(i => new
                    {
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductCode = i.Product.Code,
                        StoreId = i.StoreId,
                        StoreNameAr = i.Store.NameAr,
                        StoreNameEn = i.Store.NameEn,
                        StoreCode = i.Store.Code,
                        CurrentQuantity = i.Quantity,
                        Threshold = threshold
                    })
                    .ToListAsync();

                return Ok(lowStockProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/inventory")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductInventory(int id)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == id)
                    .Select(i => new
                    {
                        StoreId = i.StoreId,
                        StoreNameAr = i.Store.NameAr,
                        StoreNameEn = i.Store.NameEn,
                        StoreCode = i.Store.Code,
                        Quantity = i.Quantity
                    })
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ValidateStock")]
        public async Task<ActionResult<object>> ValidateStock(int productId, int storeId, decimal quantity)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == storeId)
                    .FirstOrDefaultAsync();

                var availableQuantity = inventory?.Quantity ?? 0;
                var isValid = availableQuantity >= quantity;

                return Ok(new
                {
                    isValid = isValid,
                    availableQuantity = availableQuantity,
                    requestedQuantity = quantity,
                    shortage = isValid ? 0 : quantity - availableQuantity,
                    message = isValid ? "Stock available" : $"Insufficient stock. Available: {availableQuantity}, Requested: {quantity}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isValid = false, message = ex.Message });
            }
        }

        [HttpPost("validate-stock")]
        public async Task<ActionResult<object>> ValidateStockPost([FromBody] StockValidationRequest request)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == request.ProductId && i.StoreId == request.StoreId)
                    .FirstOrDefaultAsync();

                var availableQuantity = inventory?.Quantity ?? 0;
                var isValid = availableQuantity >= request.RequestedQuantity;

                return Ok(new
                {
                    IsValid = isValid,
                    AvailableQuantity = availableQuantity,
                    RequestedQuantity = request.RequestedQuantity,
                    Shortage = isValid ? 0 : request.RequestedQuantity - availableQuantity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{productId}/variant-stock")]
        public async Task<ActionResult<object>> GetVariantStockInfo(int productId, int variantId, int storeId)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == storeId)
                    .FirstOrDefaultAsync();

                var variant = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.Id == variantId)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                {
                    return Ok(new
                    {
                        ProductId = productId,
                        VariantId = variantId,
                        StoreId = storeId,
                        AvailableQuantity = 0,
                        ReorderLevel = variant?.ReorderLevel ?? 0,
                        IsLowStock = true
                    });
                }

                var isLowStock = inventory.Quantity <= (variant?.ReorderLevel ?? 0);

                return Ok(new
                {
                    ProductId = productId,
                    VariantId = variantId,
                    StoreId = storeId,
                    AvailableQuantity = inventory.Quantity,
                    ReorderLevel = variant?.ReorderLevel ?? 0,
                    IsLowStock = isLowStock
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<List<object>> GetProductVariantsStockInternal(int productId, int storeId)
        {
            try
            {
                var variants = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.ProductId == productId)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var variant in variants)
                {
                    // Get inventory for this specific variant in the specified store
                    var variantInventory = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.ProductId == productId && i.StoreId == storeId && i.VariantId == variant.Id)
                        .FirstOrDefaultAsync();

                    result.Add(new
                    {
                        ProductId = productId,
                        VariantId = variant.Id,
                        StoreId = storeId,
                        AvailableQuantity = variantInventory?.Quantity ?? 0, // Variant-specific quantity
                        ReorderLevel = variant.ReorderLevel ?? 0,
                        IsLowStock = (variantInventory?.Quantity ?? 0) <= (variant.ReorderLevel ?? 0),
                        LastUpdated = variantInventory?.UpdatedAt
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                return new List<object>();
            }
        }

        [HttpGet("{productId}/variants-stock")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductVariantsStock(int productId, int storeId)
        {
            try
            {
                var variants = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.ProductId == productId)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var variant in variants)
                {
                    // Get inventory for this specific variant in the specified store
                    var variantInventory = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.ProductId == productId && i.StoreId == storeId && i.VariantId == variant.Id)
                        .FirstOrDefaultAsync();

                    result.Add(new
                    {
                        ProductId = productId,
                        VariantId = variant.Id,
                        StoreId = storeId,
                        AvailableQuantity = variantInventory?.Quantity ?? 0, // Variant-specific quantity
                        ReorderLevel = variant.ReorderLevel ?? 0,
                        IsLowStock = (variantInventory?.Quantity ?? 0) <= (variant.ReorderLevel ?? 0),
                        LastUpdated = variantInventory?.UpdatedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{productId}/variant-stock")]
        public async Task<ActionResult> UpdateVariantStock(int productId, [FromBody] VariantStockUpdateRequest request)
        {
            try
            {
                // For shared stock approach, we update the main product's inventory
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                {
                    // Create new inventory record if it doesn't exist
                    inventory = new Warehousing.Data.Entities.Inventory
                    {
                        ProductId = productId,
                        StoreId = request.StoreId,
                        Quantity = 0
                    };
                    await _unitOfWork.InventoryRepo.CreateAsync(inventory);
                }

                // Update the quantity
                inventory.Quantity += request.QuantityChange;

                // Ensure quantity doesn't go below zero
                if (inventory.Quantity < 0)
                {
                    inventory.Quantity = 0;
                }

                await _unitOfWork.InventoryRepo.UpdateAsync(inventory);

                // Create inventory transaction record
                var transaction = new Warehousing.Data.Entities.InventoryTransaction
                {
                    ProductId = productId,
                    StoreId = request.StoreId,
                    QuantityChanged = request.QuantityChange,
                    QuantityBefore = inventory.Quantity - request.QuantityChange,
                    QuantityAfter = inventory.Quantity,
                    TransactionDate = DateTime.UtcNow,
                    Notes = request.Notes ?? $"Variant stock update for variant {request.VariantId}",
                    TransactionTypeId = 3, // Adjustment transaction type
                    UnitCost = 0 // Will be updated based on product cost
                };

                await _unitOfWork.InventoryTransactionRepo.CreateAsync(transaction);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, newQuantity = inventory.Quantity });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{productId}/low-stock-variants")]
        public async Task<ActionResult<IEnumerable<object>>> GetLowStockVariants(int productId, int storeId)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == storeId)
                    .FirstOrDefaultAsync();

                var inventoryQuantity = inventory?.Quantity ?? 0;
                var variants = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.ProductId == productId)
                    .Where(v => inventoryQuantity <= (v.ReorderLevel ?? 0))
                    .ToListAsync();

                var result = variants.Select(v => new
                {
                    ProductId = productId,
                    VariantId = v.Id,
                    StoreId = storeId,
                    AvailableQuantity = inventory?.Quantity ?? 0,
                    ReorderLevel = v.ReorderLevel ?? 0,
                    IsLowStock = true
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{productId}/set-variant-stock")]
        public async Task<ActionResult> SetVariantStock(int productId, [FromBody] SetVariantStockRequest request)
        {
            try
            {
                // Get or create inventory record for this specific variant
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId && i.VariantId == request.VariantId)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                {
                    // Create new inventory record for this variant
                    inventory = new Warehousing.Data.Entities.Inventory
                    {
                        ProductId = productId,
                        StoreId = request.StoreId,
                        VariantId = request.VariantId,
                        Quantity = request.Quantity
                    };
                    await _unitOfWork.InventoryRepo.CreateAsync(inventory);
                }
                else
                {
                    // Update existing inventory record
                    inventory.Quantity = request.Quantity;
                    await _unitOfWork.InventoryRepo.UpdateAsync(inventory);
                }

                await _unitOfWork.SaveAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Stock quantity set to {request.Quantity}",
                    newQuantity = request.Quantity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{productId}/distribute-stock-to-variants")]
        public async Task<ActionResult> DistributeStockToVariants(int productId, [FromBody] DistributeStockRequest request)
        {
            try
            {
                // Get the main product inventory
                var mainInventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId && i.VariantId == null)
                    .FirstOrDefaultAsync();

                if (mainInventory == null)
                {
                    return BadRequest("Main product inventory not found");
                }

                // Get all variants for this product
                var variants = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.ProductId == productId)
                    .ToListAsync();

                if (variants.Count == 0)
                {
                    return BadRequest("No variants found for this product");
                }

                // Calculate total requested distribution
                var totalRequested = request.VariantDistributions.Sum(d => d.Quantity);
                if (totalRequested > mainInventory.Quantity)
                {
                    return BadRequest($"Total requested quantity ({totalRequested}) exceeds available stock ({mainInventory.Quantity})");
                }

                // Create or update inventory records for each variant
                foreach (var distribution in request.VariantDistributions)
                {
                    var variant = variants.FirstOrDefault(v => v.Id == distribution.VariantId);
                    if (variant == null) continue;

                    // Get or create inventory record for this variant
                    var variantInventory = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId && i.VariantId == distribution.VariantId)
                        .FirstOrDefaultAsync();

                    if (variantInventory == null)
                    {
                        // Create new inventory record for this variant
                        variantInventory = new Warehousing.Data.Entities.Inventory
                        {
                            ProductId = productId,
                            StoreId = request.StoreId,
                            VariantId = distribution.VariantId,
                            Quantity = distribution.Quantity
                        };
                        await _unitOfWork.InventoryRepo.CreateAsync(variantInventory);
                    }
                    else
                    {
                        // Update existing inventory record
                        variantInventory.Quantity = distribution.Quantity;
                        await _unitOfWork.InventoryRepo.UpdateAsync(variantInventory);
                    }
                }

                // Update main product inventory (reduce by distributed amount)
                mainInventory.Quantity -= totalRequested;
                await _unitOfWork.InventoryRepo.UpdateAsync(mainInventory);

                await _unitOfWork.SaveAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Successfully distributed {totalRequested} units to variants",
                    remainingMainStock = mainInventory.Quantity
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SplitGeneralToVariants")]
        public async Task<IActionResult> SplitGeneralToVariants([FromBody] SplitGeneralToVariantsRequest request)
        {
            var strategy = _unitOfWork.GetExecutionStrategy();
            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var generalInventory = await _unitOfWork.InventoryRepo.GetByIdAsync(request.GeneralInventoryId);
                    if (generalInventory == null || generalInventory.Quantity < request.GeneralQuantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("General inventory not found or insufficient quantity.");
                    }

                    var totalAlloc = request.Allocations.Sum(a => a.Quantity);
                    if (totalAlloc > generalInventory.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Allocation exceeds available general quantity. (Available: {generalInventory.Quantity}, Requested: {totalAlloc})");
                    }

                    foreach (var alloc in request.Allocations)
                    {
                        var variantInventory = await _unitOfWork.InventoryRepo
                            .GetByCondition(i => i.ProductId == request.ProductId && i.StoreId == request.StoreId && i.VariantId == alloc.VariantId)
                            .FirstOrDefaultAsync();
                        if (variantInventory == null)
                        {
                            variantInventory = new Warehousing.Data.Entities.Inventory
                            {
                                ProductId = request.ProductId,
                                StoreId = request.StoreId,
                                VariantId = alloc.VariantId,
                                Quantity = alloc.Quantity
                            };
                            await _unitOfWork.InventoryRepo.CreateAsync(variantInventory);
                        }
                        else
                        {
                            variantInventory.Quantity += alloc.Quantity;
                            await _unitOfWork.InventoryRepo.UpdateAsync(variantInventory);
                        }

                        // Create inventory transaction record for audit
                        var transactionType = await _unitOfWork.Context.Set<Warehousing.Data.Entities.TransactionType>()
                            .FirstOrDefaultAsync(t => t.Code == "ADJUSTMENT" || t.Id == 3);
                        
                        if (transactionType != null)
                        {
                            var invTransaction = new Warehousing.Data.Entities.InventoryTransaction
                            {
                                ProductId = request.ProductId,
                                StoreId = request.StoreId,
                                VariantId = alloc.VariantId,
                                QuantityChanged = alloc.Quantity,
                                QuantityBefore = variantInventory.Quantity - alloc.Quantity,
                                QuantityAfter = variantInventory.Quantity,
                                TransactionDate = DateTime.UtcNow,
                                Notes = $"Allocated {alloc.Quantity} from general inventory to variant {alloc.VariantId}",
                                TransactionTypeId = transactionType.Id,
                                UnitCost = 0
                            };
                            await _unitOfWork.InventoryTransactionRepo.CreateAsync(invTransaction);
                        }
                    }

                    generalInventory.Quantity -= totalAlloc;
                    await _unitOfWork.InventoryRepo.UpdateAsync(generalInventory);
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                    return Ok(new {
                        success = true,
                        message = "Quantities allocated successfully to variants.",
                        remainingGeneral = generalInventory.Quantity
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(ex.Message);
                }
            });
        }

        [HttpPost("{productId}/recall-stock-from-variant")]
        public async Task<ActionResult> RecallStockFromVariant(int productId, [FromBody] RecallStockRequest request)
        {
            try
            {
                // Find inventory entry for the variant
                var variantInventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId && i.VariantId == request.VariantId)
                    .FirstOrDefaultAsync();

                if (variantInventory == null || variantInventory.Quantity < request.Quantity)
                    return BadRequest("Not enough quantity in the selected variant to recall");

                // Find or create inventory entry for the general product
                var generalInventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId && i.StoreId == request.StoreId && (i.VariantId == null || i.VariantId == 0))
                    .FirstOrDefaultAsync();
                if (generalInventory == null)
                {
                    generalInventory = new Warehousing.Data.Entities.Inventory
                    {
                        ProductId = productId,
                        StoreId = request.StoreId,
                        Quantity = 0
                    };
                    await _unitOfWork.InventoryRepo.CreateAsync(generalInventory);
                }

                // Perform the recall
                variantInventory.Quantity -= request.Quantity;
                generalInventory.Quantity += request.Quantity;

                await _unitOfWork.InventoryRepo.UpdateAsync(variantInventory);
                await _unitOfWork.InventoryRepo.UpdateAsync(generalInventory);

                // Transaction log for both operations
                var recallTransaction = new Warehousing.Data.Entities.InventoryTransaction
                {
                    ProductId = productId,
                    StoreId = request.StoreId,
                    VariantId = request.VariantId,
                    QuantityChanged = -request.Quantity,
                    QuantityBefore = variantInventory.Quantity + request.Quantity,
                    QuantityAfter = variantInventory.Quantity,
                    TransactionDate = DateTime.UtcNow,
                    Notes = request.Notes ?? $"Stock recalled from variant {request.VariantId}",
                    TransactionTypeId = 4, // type 4 for recall (define as needed)
                    UnitCost = 0
                };
                await _unitOfWork.InventoryTransactionRepo.CreateAsync(recallTransaction);

                var addToGeneralTransaction = new Warehousing.Data.Entities.InventoryTransaction
                {
                    ProductId = productId,
                    StoreId = request.StoreId,
                    VariantId = null,
                    QuantityChanged = request.Quantity,
                    QuantityBefore = generalInventory.Quantity - request.Quantity,
                    QuantityAfter = generalInventory.Quantity,
                    TransactionDate = DateTime.UtcNow,
                    Notes = request.Notes ?? $"Stock recalled to general from variant {request.VariantId}",
                    TransactionTypeId = 4,
                    UnitCost = 0
                };
                await _unitOfWork.InventoryTransactionRepo.CreateAsync(addToGeneralTransaction);

                await _unitOfWork.SaveAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class RecallStockRequest
        {
            public int StoreId { get; set; }
            public int VariantId { get; set; }
            public decimal Quantity { get; set; }
            public string? Notes { get; set; }
        }
    }

    public class StockValidationRequest
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public decimal RequestedQuantity { get; set; }
    }

    public class VariantStockUpdateRequest
    {
        public int VariantId { get; set; }
        public int StoreId { get; set; }
        public decimal QuantityChange { get; set; }
        public string? Notes { get; set; }
    }

    public class DistributeStockRequest
    {
        public int StoreId { get; set; }
        public List<VariantDistribution> VariantDistributions { get; set; } = new List<VariantDistribution>();
    }

    public class VariantDistribution
    {
        public int VariantId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class SetVariantStockRequest
    {
        public int VariantId { get; set; }
        public int StoreId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class SplitGeneralToVariantsRequest
    {
        public int GeneralInventoryId { get; set; }
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public decimal GeneralQuantity { get; set; }
        public List<VariantDistribution> Allocations { get; set; } = new List<VariantDistribution>();
    }
}