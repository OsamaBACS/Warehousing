using Warehousing.Data.Entities;
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
    public class StoresController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoresController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetStores")]
        public async Task<IActionResult> GetStores()
        {
            try
            {
                var list = await _unitOfWork.StoreRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStoreById")]
        public async Task<IActionResult> GetStoreById(int Id)
        {
            try
            {
                var store = await _unitOfWork.StoreRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (store == null)
                {
                    return NotFound("Store Not Found!");
                }
                else
                {
                    return Ok(store);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveStore")]
        public async Task<IActionResult> SaveStore(StoreDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Store Model is null!");
                }

                var isStoreExist = await _unitOfWork.StoreRepo
                    .GetByCondition(r => (r.NameEn == dto.NameEn || r.NameAr == dto.NameAr) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isStoreExist != null)
                    return BadRequest("Store already exists.");

                if (dto.Id > 0)
                {
                    var StoreToUpdate = _unitOfWork.StoreRepo.GetByCondition(r => r.Id == dto.Id).FirstOrDefault();
                    if (StoreToUpdate != null)
                    {
                        _mapper.Map(dto, StoreToUpdate);
                        var result = await _unitOfWork.StoreRepo.UpdateAsync(StoreToUpdate);
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound("This Store is not exist!");
                    }
                }
                else
                {
                    var store = new Store();
                    _mapper.Map(dto, store);
                    var result = await _unitOfWork.StoreRepo.CreateAsync(store);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Store");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<StoreSimpleDto>>> GetActiveStores()
        {
            try
            {
                var stores = await _unitOfWork.StoreRepo
                    .GetByCondition(s => s.IsActive)
                    .ProjectTo<StoreSimpleDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(stores);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("warehouses")]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetWarehouses()
        {
            try
            {
                var warehouses = await _unitOfWork.StoreRepo
                    .GetByCondition(s => s.IsMainWarehouse && s.IsActive)
                    .ProjectTo<StoreDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(warehouses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/inventory-summary")]
        public async Task<ActionResult<object>> GetStoreInventorySummary(int id)
        {
            try
            {
                var summary = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.StoreId == id)
                    .GroupBy(i => i.StoreId)
                    .Select(g => new
                    {
                        StoreId = g.Key,
                        TotalProducts = g.Count(),
                        TotalQuantity = g.Sum(i => i.Quantity),
                        LowStockProducts = g.Count(i => i.Quantity <= 10),
                        ZeroStockProducts = g.Count(i => i.Quantity == 0)
                    })
                    .FirstOrDefaultAsync();

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<object>>> GetStoreProducts(int id)
        {
            try
            {
                var products = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.StoreId == id)
                    .Select(i => new
                    {
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductCode = i.Product.Code,
                        Quantity = i.Quantity,
                        UnitNameAr = i.Product.Unit.NameAr,
                        UnitNameEn = i.Product.Unit.NameEn
                    })
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<StoreDto>> GetStoreByCode(string code)
        {
            try
            {
                var store = await _unitOfWork.StoreRepo
                    .GetByCondition(s => s.Code == code)
                    .ProjectTo<StoreDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (store == null)
                    return NotFound();

                return Ok(store);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStore(int id)
        {
            try
            {
                // Check if store has inventory
                var hasInventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.StoreId == id)
                    .AnyAsync();

                if (hasInventory)
                    return BadRequest("Cannot delete store with existing inventory. Please transfer inventory first.");

                await _unitOfWork.StoreRepo.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}