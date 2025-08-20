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
                    .Include(u => u.Store)
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
                var product = await _unitOfWork.ProductRepo.GetAll()
                    .Where(p => p.SubCategoryId == SubCategoryId && p.IsActive)
                    .Include(ps => ps.SubCategory)
                    .Include(ps => ps.Unit)
                    .Include(ps => ps.Store)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Code = p.Code,
                        NameAr = p.NameAr,
                        NameEn = p.NameEn,
                        Description = p.Description,
                        OpeningBalance = p.OpeningBalance,
                        ReorderLevel = p.ReorderLevel,
                        QuantityInStock = p.QuantityInStock,
                        CostPrice = p.CostPrice,
                        SellingPrice = p.SellingPrice,
                        IsActive = p.IsActive,
                        SubCategoryId = p.SubCategoryId,
                        SubCategory = _mapper.Map<SubCategoryDto>(p.SubCategory),
                        UnitId = p.UnitId,
                        Unit = _mapper.Map<UnitDto>(p.Unit),
                        Store = _mapper.Map<StoreDto>(p.Store),
                        StoreId = p.StoreId,
                        ImagePath = p.ImagePath
                    })
                    .ToListAsync();



                return Ok(product);
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
                    .Include(p => p.Store);

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
                    OpeningBalance = p.OpeningBalance,
                    ReorderLevel = p.ReorderLevel,
                    QuantityInStock = p.QuantityInStock,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    ImagePath = p.ImagePath,
                    IsActive = p.IsActive,
                    SubCategoryId = p.SubCategoryId,
                    SubCategory = _mapper.Map<SubCategoryDto>(p.SubCategory),
                    UnitId = p.UnitId,
                    Unit = _mapper.Map<UnitDto>(p.Unit),
                    Store = _mapper.Map<StoreDto>(p.Store),
                    StoreId = p.StoreId,
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

                var test = _unitOfWork.ProductRepo.GetByCondition(x => x.StoreId == StoreId).ToList();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.NameAr.Contains(keyword) || x.NameEn.Contains(keyword));
                }

                if (StoreId > 0)
                {
                    query = query.Where(x => x.StoreId == StoreId);
                }

                // Get total count *before* pagination
                var totalSize = await query.CountAsync();

                // Apply includes and projection
                var list = await query
                    .Include(c => c.SubCategory)
                    .Include(u => u.Unit)
                    .Include(p => p.Store)
                    .OrderBy(x => x.Id)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Code = p.Code,
                        NameEn = p.NameEn,
                        NameAr = p.NameAr,
                        Description = p.Description ?? string.Empty,
                        OpeningBalance = p.OpeningBalance,
                        ReorderLevel = p.ReorderLevel,
                        QuantityInStock = p.QuantityInStock,
                        CostPrice = p.CostPrice,
                        SellingPrice = p.SellingPrice,
                        ImagePath = p.ImagePath,
                        IsActive = p.IsActive,
                        SubCategoryId = p.SubCategoryId,
                        SubCategory = _mapper.Map<SubCategoryDto>(p.SubCategory),
                        UnitId = p.UnitId,
                        Unit = _mapper.Map<UnitDto>(p.Unit),
                        Store = _mapper.Map<StoreDto>(p.Store),
                        StoreId = p.StoreId,
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
    }
}