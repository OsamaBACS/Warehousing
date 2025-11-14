using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductVariantsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("by-product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var list = await _unitOfWork.ProductVariantRepo
                .GetByCondition(v => v.ProductId == productId)
                .ProjectTo<ProductVariantSimpleDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var entity = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(_mapper.Map<ProductVariantDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductVariantDto dto)
        {
            var entity = _mapper.Map<Warehousing.Data.Entities.ProductVariant>(dto);
            var created = await _unitOfWork.ProductVariantRepo.CreateAsync(entity);
            await _unitOfWork.SaveAsync();

            // Don't auto-create inventory records - let them be created when stock is actually added

            return Ok(_mapper.Map<ProductVariantDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantDto dto)
        {
            var entity = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _unitOfWork.ProductVariantRepo.UpdateAsync(entity);
            await _unitOfWork.SaveAsync();
            return Ok(_mapper.Map<ProductVariantDto>(entity));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfWork.ProductVariantRepo.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return Ok(new { success = true });
        }

        [HttpGet("{id}/validate-stock")]
        public async Task<ActionResult<object>> ValidateVariantStock(int id, int storeId, decimal quantity)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
                if (variant == null) return NotFound();

                // Check variant stock from Inventory table
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == variant.ProductId && i.StoreId == storeId && i.VariantId == id)
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

        [HttpGet("{id}/stock-info")]
        public async Task<ActionResult<object>> GetVariantStockInfo(int id, int storeId)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
                if (variant == null) return NotFound();

                // Get variant stock from Inventory table
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == variant.ProductId && i.StoreId == storeId && i.VariantId == id)
                    .FirstOrDefaultAsync();

                var availableQuantity = inventory?.Quantity ?? 0;
                var reorderLevel = variant.ReorderLevel ?? 0;
                var isLowStock = availableQuantity <= reorderLevel;

                return Ok(new
                {
                    VariantId = id,
                    ProductId = variant.ProductId,
                    StoreId = storeId,
                    AvailableQuantity = availableQuantity,
                    ReorderLevel = reorderLevel,
                    IsLowStock = isLowStock
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/update-stock")]
        public async Task<ActionResult> UpdateVariantStock(int id, [FromBody] VariantStockUpdateRequest request)
        {
            try
            {
                var variant = await _unitOfWork.ProductVariantRepo.GetByIdAsync(id);
                if (variant == null) return NotFound();

                // Get or create inventory record for this variant
                var inventory = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.ProductId == variant.ProductId && i.StoreId == request.StoreId && i.VariantId == id)
                    .FirstOrDefaultAsync();

                if (inventory == null)
                {
                    // Create new inventory record for this variant
                    inventory = new Warehousing.Data.Entities.Inventory
                    {
                        ProductId = variant.ProductId,
                        StoreId = request.StoreId,
                        VariantId = id,
                        Quantity = 0
                    };
                    await _unitOfWork.InventoryRepo.CreateAsync(inventory);
                }

                // Update inventory quantity
                var currentStock = inventory.Quantity;
                var newStock = currentStock + request.QuantityChange;

                // Ensure stock doesn't go below zero
                if (newStock < 0)
                {
                    newStock = 0;
                }

                inventory.Quantity = newStock;
                await _unitOfWork.InventoryRepo.UpdateAsync(inventory);

                // Create inventory transaction record
                var transaction = new Warehousing.Data.Entities.InventoryTransaction
                {
                    ProductId = variant.ProductId,
                    StoreId = request.StoreId,
                    QuantityChanged = request.QuantityChange,
                    QuantityBefore = currentStock,
                    QuantityAfter = newStock,
                    TransactionDate = DateTime.UtcNow,
                    Notes = request.Notes ?? $"Variant stock update for {variant.Name}",
                    TransactionTypeId = 3, // Adjustment transaction type
                    UnitCost = 0 // Will be updated based on product cost
                };

                await _unitOfWork.InventoryTransactionRepo.CreateAsync(transaction);
                await _unitOfWork.SaveAsync();

                return Ok(new { success = true, newQuantity = newStock });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
