using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventory()
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetAll()
                    .ProjectTo<InventoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-store/{storeId}")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByStore(int storeId)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.StoreId == storeId)
                    .ProjectTo<InventoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("by-product/{productId}")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByProduct(int productId)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == productId)
                    .ProjectTo<InventoryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetInventorySummary()
        {
            try
            {
                var summary = await _unitOfWork.InventoryRepo
                    .GetAll()
                    .GroupBy(i => 1)
                    .Select(g => new
                    {
                        TotalProducts = g.Select(i => i.ProductId).Distinct().Count(),
                        TotalStores = g.Select(i => i.StoreId).Distinct().Count(),
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

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<object>>> GetLowStockItems([FromQuery] decimal threshold = 10)
        {
            try
            {
                var lowStockItems = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.Quantity <= threshold)
                    .Select(i => new
                    {
                        InventoryId = i.Id,
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductCode = i.Product.Code,
                        StoreId = i.StoreId,
                        StoreNameAr = i.Store.NameAr,
                        StoreNameEn = i.Store.NameEn,
                        StoreCode = i.Store.Code,
                        CurrentQuantity = i.Quantity,
                        Threshold = threshold,
                        IsZero = i.Quantity == 0
                    })
                    .OrderBy(i => i.CurrentQuantity)
                    .ToListAsync();

                return Ok(lowStockItems);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("adjust")]
        public async Task<ActionResult<InventoryDto>> AdjustInventory([FromBody] InventoryAdjustmentRequest request)
        {
            try
            {
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == request.ProductId && i.StoreId == request.StoreId)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                {
                    return NotFound("Inventory record not found for this product and store combination.");
                }

                var quantityBefore = inventory.Quantity;
                inventory.Quantity += request.AdjustmentQuantity;
                var quantityAfter = inventory.Quantity;

                // Create inventory transaction record
                var transactionType = request.AdjustmentQuantity > 0 
                    ? await _unitOfWork.TransactionTypeRepo.GetByCondition(tt => tt.Code == "ADJUSTMENT_PLUS").FirstOrDefaultAsync()
                    : await _unitOfWork.TransactionTypeRepo.GetByCondition(tt => tt.Code == "ADJUSTMENT_MINUS").FirstOrDefaultAsync();

                if (transactionType != null)
                {
                    var transaction = new InventoryTransaction
                    {
                        ProductId = request.ProductId,
                        StoreId = request.StoreId,
                        TransactionTypeId = transactionType.Id,
                        QuantityChanged = request.AdjustmentQuantity,
                        QuantityBefore = quantityBefore,
                        QuantityAfter = quantityAfter,
                        UnitCost = request.UnitCost,
                        TransactionDate = DateTime.UtcNow,
                        Notes = request.Notes ?? $"Manual inventory adjustment"
                    };

                    await _unitOfWork.InventoryTransactionRepo.CreateAsync(transaction);
                }

                var updatedInventory = await _unitOfWork.InventoryRepo.UpdateAsync(inventory);
                await _unitOfWork.SaveAsync();

                return Ok(_mapper.Map<InventoryDto>(updatedInventory));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk-adjust")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> BulkAdjustInventory([FromBody] List<InventoryAdjustmentRequest> requests)
        {
            try
            {
                var results = new List<InventoryDto>();

                foreach (var request in requests)
                {
                    var inventory = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.ProductId == request.ProductId && i.StoreId == request.StoreId)
                        .FirstOrDefaultAsync();

                    if (inventory == null)
                        continue; // Skip if inventory not found

                    var quantityBefore = inventory.Quantity;
                    inventory.Quantity += request.AdjustmentQuantity;
                    var quantityAfter = inventory.Quantity;

                    // Create inventory transaction record
                    var transactionType = request.AdjustmentQuantity > 0 
                        ? await _unitOfWork.TransactionTypeRepo.GetByCondition(tt => tt.Code == "ADJUSTMENT_PLUS").FirstOrDefaultAsync()
                        : await _unitOfWork.TransactionTypeRepo.GetByCondition(tt => tt.Code == "ADJUSTMENT_MINUS").FirstOrDefaultAsync();

                    if (transactionType != null)
                    {
                        var transaction = new InventoryTransaction
                        {
                            ProductId = request.ProductId,
                            StoreId = request.StoreId,
                            TransactionTypeId = transactionType.Id,
                            QuantityChanged = request.AdjustmentQuantity,
                            QuantityBefore = quantityBefore,
                            QuantityAfter = quantityAfter,
                            UnitCost = request.UnitCost,
                            TransactionDate = DateTime.UtcNow,
                            Notes = request.Notes ?? "Bulk inventory adjustment"
                        };

                        await _unitOfWork.InventoryTransactionRepo.CreateAsync(transaction);
                    }

                    var updatedInventory = await _unitOfWork.InventoryRepo.UpdateAsync(inventory);
                    results.Add(_mapper.Map<InventoryDto>(updatedInventory));
                }

                await _unitOfWork.SaveAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class InventoryAdjustmentRequest
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public decimal AdjustmentQuantity { get; set; } // Positive for increase, negative for decrease
        public decimal UnitCost { get; set; }
        public string? Notes { get; set; }
    }
}
