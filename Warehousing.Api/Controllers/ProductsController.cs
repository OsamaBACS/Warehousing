using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    .Include(ps => ps.Inventories)
                    .ProjectTo<ProductSimpleDto>(_mapper.ConfigurationProvider)
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
                    .Include(u => u.Inventories).ThenInclude(s => s.Store);

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
                    ImagePath = p.ImagePath,
                    IsActive = p.IsActive,
                    SubCategoryId = p.SubCategoryId,
                    SubCategory = _mapper.Map<SubCategoryDto>(p.SubCategory),
                    UnitId = p.UnitId,
                    Unit = _mapper.Map<UnitDto>(p.Unit),
                    Inventories = _mapper.Map<List<InventoryDto>>(p.Inventories),
                }).ToList();

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
                        ImagePath = p.ImagePath,
                        IsActive = p.IsActive,
                        SubCategoryId = p.SubCategoryId,
                        SubCategory = _mapper.Map<SubCategoryDto>(p.SubCategory),
                        UnitId = p.UnitId,
                        Unit = _mapper.Map<UnitDto>(p.Unit),
                        Inventories = _mapper.Map<List<InventoryDto>>(p.Inventories),
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
    }

    public class StockValidationRequest
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public decimal RequestedQuantity { get; set; }
    }
}