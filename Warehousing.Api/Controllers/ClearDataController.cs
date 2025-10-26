using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClearDataController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClearDataController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("clear-inventory-variants-modifiers")]
        public async Task<IActionResult> ClearInventoryVariantsModifiers()
        {
            try
            {
                // Clear in order to respect foreign key constraints
                
                // 1. Clear OrderItemModifiers first (has foreign key to modifiers)
                var orderItemModifiers = await _unitOfWork.OrderItemModifierRepo.GetAll().ToListAsync();
                foreach (var item in orderItemModifiers)
                {
                    await _unitOfWork.OrderItemModifierRepo.DeleteAsync(item.Id);
                }

                // 2. Clear InventoryTransactions
                var inventoryTransactions = await _unitOfWork.InventoryTransactionRepo.GetAll().ToListAsync();
                foreach (var transaction in inventoryTransactions)
                {
                    await _unitOfWork.InventoryTransactionRepo.DeleteAsync(transaction.Id);
                }

                // 3. Clear Inventories
                var inventories = await _unitOfWork.InventoryRepo.GetAll().ToListAsync();
                foreach (var inventory in inventories)
                {
                    await _unitOfWork.InventoryRepo.DeleteAsync(inventory.Id);
                }

                // 4. Clear ProductModifierGroups (has foreign key to modifiers)
                var modifierGroups = await _unitOfWork.ProductModifierGroupRepo.GetAll().ToListAsync();
                foreach (var group in modifierGroups)
                {
                    await _unitOfWork.ProductModifierGroupRepo.DeleteAsync(group.Id);
                }

                // 5. Clear ProductModifierOptions (has foreign key to modifiers)
                var modifierOptions = await _unitOfWork.ProductModifierOptionRepo.GetAll().ToListAsync();
                foreach (var option in modifierOptions)
                {
                    await _unitOfWork.ProductModifierOptionRepo.DeleteAsync(option.Id);
                }

                // 6. Clear ProductModifiers
                var modifiers = await _unitOfWork.ProductModifierRepo.GetAll().ToListAsync();
                foreach (var modifier in modifiers)
                {
                    await _unitOfWork.ProductModifierRepo.DeleteAsync(modifier.Id);
                }

                // 7. Clear ProductVariants
                var variants = await _unitOfWork.ProductVariantRepo.GetAll().ToListAsync();
                foreach (var variant in variants)
                {
                    await _unitOfWork.ProductVariantRepo.DeleteAsync(variant.Id);
                }

                await _unitOfWork.SaveAsync();

                // Get counts for summary
                var remainingProducts = await _unitOfWork.ProductRepo.GetAll().CountAsync();
                var remainingInventories = await _unitOfWork.InventoryRepo.GetAll().CountAsync();
                var remainingVariants = await _unitOfWork.ProductVariantRepo.GetAll().CountAsync();
                var remainingModifiers = await _unitOfWork.ProductModifierRepo.GetAll().CountAsync();

                return Ok(new
                {
                    success = true,
                    message = "Successfully cleared inventory, variants, and modifiers while preserving products",
                    summary = new
                    {
                        productsPreserved = remainingProducts,
                        inventoriesCleared = inventories.Count,
                        variantsCleared = variants.Count,
                        modifiersCleared = modifiers.Count,
                        modifierOptionsCleared = modifierOptions.Count,
                        modifierGroupsCleared = modifierGroups.Count,
                        inventoryTransactionsCleared = inventoryTransactions.Count,
                        orderItemModifiersCleared = orderItemModifiers.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("data-summary")]
        public async Task<IActionResult> GetDataSummary()
        {
            try
            {
                var summary = new
                {
                    products = await _unitOfWork.ProductRepo.GetAll().CountAsync(),
                    inventories = await _unitOfWork.InventoryRepo.GetAll().CountAsync(),
                    variants = await _unitOfWork.ProductVariantRepo.GetAll().CountAsync(),
                    modifiers = await _unitOfWork.ProductModifierRepo.GetAll().CountAsync(),
                    modifierOptions = await _unitOfWork.ProductModifierOptionRepo.GetAll().CountAsync(),
                    modifierGroups = await _unitOfWork.ProductModifierGroupRepo.GetAll().CountAsync(),
                    inventoryTransactions = await _unitOfWork.InventoryTransactionRepo.GetAll().CountAsync(),
                    orderItemModifiers = await _unitOfWork.OrderItemModifierRepo.GetAll().CountAsync()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
