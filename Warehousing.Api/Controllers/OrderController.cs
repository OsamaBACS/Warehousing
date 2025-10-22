using System.Security.Claims;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Dtos.Filters;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetOrdersPagination")]
        public async Task<ActionResult> GetOrdersPagination(int pageIndex, int pageSize, int OrderTypeId = 0)
        {
            try
            {
                var orders = await _unitOfWork.OrderRepo
                    .GetAllPagination(
                        pageIndex,
                        pageSize,
                        x => x.Id,
                        OrderTypeId > 0 ? o => o.OrderTypeId == OrderTypeId : null,
                        [o => o.OrderType, c => c.Customer, p => p.Supplier, s => s.Status]
                    );

                var TotalSize = await _unitOfWork.OrderRepo.GetTotalCount();

                return Ok(new
                {
                    orders = orders,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("FilterOrdersPagination")]
        public async Task<ActionResult> FilterOrdersPagination([FromBody] OrderFilters filter)
        {
            try
            {
                var orders = _unitOfWork.OrderRepo.GetAll();

                if (filter.OrderDate != null)
                {
                    orders = orders.Where(o => o.OrderDate == filter.OrderDate);
                }

                if (filter.OrderTypeId != null)
                {
                    orders = orders.Where(o => o.OrderTypeId == filter.OrderTypeId);
                }

                if (filter.CustomerId != null)
                {
                    orders = orders.Where(o => o.CustomerId == filter.CustomerId);
                }

                if (filter.SupplierId != null)
                {
                    orders = orders.Where(o => o.SupplierId == filter.SupplierId);
                }

                if (filter.StatusId != null)
                {
                    orders = orders.Where(o => o.StatusId == filter.StatusId);
                }


                var result = await orders.OrderByDescending(o => o.OrderDate)
                         .Skip(filter.PageSize * (filter.PageIndex - 1))
                         .Take(filter.PageSize).ToListAsync();

                return Ok(new
                {
                    orders = result,
                    totals = result.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetOrderById")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _unitOfWork.OrderRepo
                    .GetAll()
                    .Include(o => o.OrderType)
                    .Include(c => c.Customer)
                    .Include(p => p.Supplier)
                    .Include(p => p.Status)
                    .Include(p => p.Items).ThenInclude(i => i.Product).ThenInclude(u => u.Unit)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (order == null) return NotFound();

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("GetOrdersByUserId")]
        public async Task<ActionResult<OrderDto>> GetOrdersByUserId([FromBody] OrderFilters filter)
        {
            try
            {
                bool isAdmin = false;
                var UserId = Convert.ToInt32(User.FindFirstValue("UserId"));
                var user = await _unitOfWork.UserRepo.GetByCondition(u => u.Id == UserId).FirstOrDefaultAsync();

                if (user == null) return NotFound("You need to login first!");

                if (user.Username == "admin")
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }

                var pending = await _unitOfWork.StatusRepo.GetByCondition(s => s.Code.Equals("PENDING")).FirstOrDefaultAsync();
                var draft = await _unitOfWork.StatusRepo.GetByCondition(s => s.Code.Equals("DRAFT")).FirstOrDefaultAsync();
                if (pending == null || draft == null) return NotFound("Please add statuses first");

                var order = _unitOfWork.OrderRepo
                    .GetAll()
                    .Include(o => o.OrderType)
                    .Include(c => c.Customer)
                    .Include(p => p.Supplier)
                    .Include(p => p.Status)
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(c => c.SubCategory)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Unit);

                if (order == null) return NotFound();

                var result = (IQueryable<Order>)order;

                // Filter by user: Admin can see all orders, regular users can only see their own orders
                if (!isAdmin)
                {
                    // For non-admin users, only show orders they created
                    result = result.Where(o => o.CreatedBy == user.Username);
                }

                // Search functionality
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    result = result.Where(o => 
                        o.Id.ToString().Contains(searchTerm) ||
                        (o.Customer != null && o.Customer.NameAr.ToLower().Contains(searchTerm)) ||
                        (o.Supplier != null && o.Supplier.Name.ToLower().Contains(searchTerm)) ||
                        (o.Items.Any(i => i.Product.NameAr.ToLower().Contains(searchTerm))) ||
                        o.TotalAmount.ToString().Contains(searchTerm)
                    );
                }

                // Date filtering
                if (filter.OrderDate != null)
                {
                    result = result.Where(o => o.OrderDate.Date == filter.OrderDate.Value.Date);
                }

                if (filter.DateFrom != null)
                {
                    result = result.Where(o => o.OrderDate >= filter.DateFrom.Value);
                }

                if (filter.DateTo != null)
                {
                    result = result.Where(o => o.OrderDate <= filter.DateTo.Value);
                }

                // Amount filtering
                if (filter.MinAmount != null)
                {
                    result = result.Where(o => o.TotalAmount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount != null)
                {
                    result = result.Where(o => o.TotalAmount <= filter.MaxAmount.Value);
                }

                // Other filters
                if (filter.OrderTypeId != null)
                {
                    result = result.Where(o => o.OrderTypeId == filter.OrderTypeId);
                }

                if (filter.CustomerId != null)
                {
                    result = result.Where(o => o.CustomerId == filter.CustomerId);
                }

                if (filter.SupplierId != null)
                {
                    result = result.Where(o => o.SupplierId == filter.SupplierId);
                }

                if (filter.StatusId != null)
                {
                    result = result.Where(o => o.StatusId == filter.StatusId);
                }


                var finalResult = await result.OrderByDescending(o => o.Id).ToListAsync();

                if (finalResult.Count > 0)
                    return Ok(new
                    {
                        orders = finalResult,
                        totals = finalResult.Count()
                    });

                return BadRequest("There are no Orders");

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("SaveOrder")]
        public async Task<IActionResult> SaveOrder(OrderDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Order Order Model is null!");
                }
                if (dto.Id > 0)
                {
                    var order = await _unitOfWork.OrderRepo.GetByConditionIncluding(po => po.Id == dto.Id, i => i.Items).FirstOrDefaultAsync();
                    if (order == null) return NotFound();

                    var incomingItemIds = dto.Items.Where(i => i.Id > 0).Select(i => i.Id).ToList();

                    var toRemove = order.Items.Where(i => !incomingItemIds.Contains(i.Id)).ToList();

                    await _unitOfWork.OrderItemRepo.DeleteRange(toRemove);

                    order.OrderDate = dto.OrderDate;
                    order.TotalAmount = dto.TotalAmount;
                    order.OrderTypeId = dto.OrderTypeId;
                    order.CustomerId = dto.CustomerId;
                    order.SupplierId = dto.SupplierId;
                    order.StatusId = dto.StatusId;

                    // Add or update items
                    foreach (var itemDto in dto.Items)
                    {
                        if (itemDto.Id > 0)
                        {
                            // Update existing item
                            var item = order.Items.First(i => i.Id == itemDto.Id);
                            item.StoreId = itemDto.StoreId;
                            item.ProductId = itemDto.ProductId;
                            item.Quantity = itemDto.Quantity;
                            item.UnitCost = itemDto.UnitCost;
                            item.UnitPrice = itemDto.UnitPrice;
                        }
                        else
                        {
                            // Add new item
                            order.Items.Add(new OrderItem
                            {
                                StoreId = itemDto.StoreId,
                                ProductId = itemDto.ProductId,
                                Quantity = itemDto.Quantity,
                                UnitCost = itemDto.UnitCost,
                                UnitPrice = itemDto.UnitPrice,
                                OrderId = dto.Id ?? 0
                            });
                        }
                    }

                    var result = await _unitOfWork.OrderRepo.UpdateAsync(order);
                    return Ok(result);
                }
                else
                {
                    // Create order first without items
                    var entity = new Order
                    {
                        OrderDate = dto.OrderDate,
                        TotalAmount = dto.TotalAmount,
                        OrderTypeId = dto.OrderTypeId,
                        CustomerId = dto.CustomerId,
                        SupplierId = dto.SupplierId,
                        StatusId = dto.StatusId,
                        Items = new List<OrderItem>() // Start with empty items
                    };

                    var result = await _unitOfWork.OrderRepo.CreateAsync(entity);
                    
                    // Now add items with the correct OrderId
                    foreach (var itemDto in dto.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = result.Id,
                            StoreId = itemDto.StoreId,
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity,
                            UnitCost = itemDto.UnitCost,
                            UnitPrice = itemDto.UnitPrice,
                            Discount = itemDto.Discount,
                            Notes = itemDto.Notes
                        };
                        
                        await _unitOfWork.OrderItemRepo.CreateAsync(orderItem);
                    }
                    
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("UpdateOrderStatusToPending/{id}")]
        public async Task<IActionResult> UpdateOrderStatusToPending(int id)
        {
            try
            {
                var order = await _unitOfWork.OrderRepo
                            .GetAll()
                            .Include(o => o.Items)
                                .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return BadRequest(new { success = false, message = "Purchase order not found." });

                if (order.StatusId == null)
                    return BadRequest(new { success = false, message = "Order status is not set." });

                var currentStatus = await _unitOfWork.StatusRepo.GetAll().FirstOrDefaultAsync(s => s.Id == order.StatusId.Value);
                if (currentStatus == null)
                    return BadRequest(new { success = false, message = "Current status not found." });

                if (currentStatus.Code == "COMPLETED")
                    return BadRequest(new { success = false, message = "Order is already COMPLETED." });

                // Get Complete status
                var pendingStatus = await _unitOfWork.StatusRepo
                    .GetByCondition(s => s.Code == "PENDING")
                    .FirstOrDefaultAsync();

                if (pendingStatus == null)
                    return BadRequest(new { success = false, message = "Pending status not configured." });

                // Update order status
                order.StatusId = pendingStatus.Id;
                // Save updated order via repository
                await _unitOfWork.OrderRepo.UpdateAsync(order);

                return Ok(new
                {
                    success = true,
                    message = "Order pending successfully.",
                    result = order
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error updating purchase order: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("UpdateOrderStatusToComplete/{id}")]
        public async Task<IActionResult> UpdateOrderStatusToComplete(int id)
        {
            try
            {
                // Use EF Core execution strategy to handle retries correctly
                var strategy = _unitOfWork.GetExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Load order with related data
                        var order = await _unitOfWork.OrderRepo
                            .GetAll()
                            .Include(o => o.Items)
                                .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(o => o.Id == id);

                        if (order == null)
                            return BadRequest(new { success = false, message = "Order not found." });

                        if (order.StatusId == null)
                            return BadRequest(new { success = false, message = "Order status is not set." });

                        var currentStatus = await _unitOfWork.StatusRepo.GetAll().FirstOrDefaultAsync(s => s.Id == order.StatusId.Value);
                        if (currentStatus == null)
                            return BadRequest(new { success = false, message = "Current status not found." });

                        if (currentStatus.Code == "COMPLETED")
                            return BadRequest(new { success = false, message = "Order is already COMPLETED." });

                        if (currentStatus.Code != "PENDING")
                            return BadRequest(new { success = false, message = "Only PENDING orders can be completed." });

                        // Get Complete status
                        var completeStatus = await _unitOfWork.StatusRepo
                            .GetByCondition(s => s.Code == "COMPLETED")
                            .FirstOrDefaultAsync();

                        if (completeStatus == null)
                            return BadRequest(new { success = false, message = "Complete status not configured." });

                        var transactionTypeCode = order.OrderTypeId == 1 ? "PURCHASE" : "SALE";
                        // Get Purchase TransactionType lookup
                        var transactionType = await _unitOfWork.TransactionTypeRepo
                            .GetByCondition(tt => tt.Code == transactionTypeCode)
                            .FirstOrDefaultAsync();

                        if (transactionType == null)
                            return BadRequest(new { success = false, message = $"Transaction type {transactionTypeCode} not configured." });

                        // Update order status
                        order.StatusId = completeStatus.Id;

                        // Save updated order via repository
                        await _unitOfWork.OrderRepo.UpdateAsync(order);

                        // STEP 1: Validate stock availability first
                        var insufficientItems = new List<string>();

                        foreach (var item in order.Items)
                        {
                            if (item.Product == null)
                                return BadRequest(new { success = false, message = $"Product not found for order item {item.Id}" });

                            // Only validate for sales (OrderTypeId != 1)
                            var inventory = item.VariantId.HasValue 
                                ? item.Product.Inventories.Where(s => s.StoreId == item.StoreId && s.VariantId == item.VariantId).FirstOrDefault()
                                : item.Product.Inventories.Where(s => s.StoreId == item.StoreId && s.VariantId == null).FirstOrDefault();
                            
                            if (order.OrderTypeId != 1 && inventory != null && inventory.Quantity < item.Quantity)
                            {
                                var variantInfo = item.VariantId.HasValue ? $" (Variant ID: {item.VariantId})" : "";
                                insufficientItems.Add($"{item.Product.NameAr}{variantInfo} (Available: {inventory.Quantity}, Requested: {item.Quantity})");
                            }
                        }

                        // If any insufficient items found → cancel before making changes
                        if (insufficientItems.Any())
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new
                            {
                                success = false,
                                message = "Order cannot be completed due to insufficient stock for the following items:",
                                insufficientItems
                            });
                        }

                        foreach (var item in order.Items)
                        {
                            // Handle inventory for variants or base product
                            var inventory = item.VariantId.HasValue 
                                ? item.Product.Inventories.Where(s => s.StoreId == item.StoreId && s.VariantId == item.VariantId).FirstOrDefault()
                                : item.Product.Inventories.Where(s => s.StoreId == item.StoreId && s.VariantId == null).FirstOrDefault();
                            
                            // Store quantities before change
                            var quantityBefore = 0m;
                            var quantityAfter = 0m;

                            if (inventory == null)
                            {
                                // For purchases, create inventory if it doesn't exist
                                if (order.OrderTypeId == 1) // Purchase → Create inventory
                                {
                                    var newInventory = new Inventory
                                    {
                                        ProductId = item.ProductId,
                                        StoreId = item.StoreId,
                                        VariantId = item.VariantId,
                                        Quantity = item.Quantity,
                                        CreatedAt = DateTime.UtcNow,
                                        CreatedBy = "System", // You might want to get this from user context
                                        UpdatedAt = DateTime.UtcNow,
                                        UpdatedBy = "System"
                                    };

                                    await _unitOfWork.InventoryRepo.CreateAsync(newInventory);
                                    
                                    quantityBefore = 0; // Starting from 0
                                    quantityAfter = item.Quantity; // Added the purchased quantity
                                }
                                else // Sale → Inventory must exist
                                {
                                    var variantInfo = item.VariantId.HasValue ? $" (Variant ID: {item.VariantId})" : "";
                                    return BadRequest(new { success = false, message = $"Inventory not found for product {item.Product.NameAr}{variantInfo} in store {item.StoreId}. Cannot sell from non-existent inventory." });
                                }
                            }
                            else
                            {
                                // Inventory exists, update it
                                quantityBefore = inventory.Quantity;
                                quantityAfter = quantityBefore;

                                if (order.OrderTypeId == 1) // Purchase → Stock In
                                {
                                    quantityAfter = quantityBefore + item.Quantity;
                                    inventory.Quantity = quantityAfter;
                                }
                                else // Sale → Stock Out (already validated)
                                {
                                    quantityAfter = quantityBefore - item.Quantity;
                                    inventory.Quantity = quantityAfter;
                                }

                                await _unitOfWork.ProductRepo.UpdateAsync(item.Product);
                            }

                            var transactionEntity = new InventoryTransaction
                            {
                                ProductId = item.ProductId,
                                StoreId = item.StoreId,
                                OrderId = order.Id,
                                OrderItemId = item.Id,
                                QuantityChanged = order.OrderTypeId == 1 ? item.Quantity : -item.Quantity,
                                QuantityBefore = quantityBefore,
                                QuantityAfter = quantityAfter,
                                UnitCost = item.UnitCost,
                                TransactionDate = DateTime.UtcNow,
                                Notes = $"{transactionTypeCode} order #{order.Id} completed.",
                                TransactionTypeId = transactionType.Id
                            };

                            await _unitOfWork.InventoryTransactionRepo.CreateAsync(transactionEntity);
                        }

                        // Save all changes (this saves everything tracked by context)
                        await _unitOfWork.SaveAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        return Ok(new
                        {
                            success = true,
                            message = $"{transactionTypeCode} order completed successfully.",
                            result = order
                        });
                    }
                    catch (Exception ex)
                    {
                        // Rollback on error
                        await transaction.RollbackAsync();
                        return StatusCode(500, new
                        {
                            success = false,
                            message = $"Error during transaction: {ex.Message}"
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error updating Order: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("UpdateApprovedOrder/{id}")]
        public async Task<IActionResult> UpdateApprovedOrder(int id, [FromBody] Order updatedOrder)
        {
            try
            {
                var strategy = _unitOfWork.GetExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Load existing order with items + products
                        var existingOrder = await _unitOfWork.OrderRepo
                            .GetAll()
                            .Include(o => o.Items)
                                .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(o => o.Id == id);

                        if (existingOrder == null)
                            return NotFound(new { success = false, message = "Order not found." });

                        // Check if order is already completed
                        var status = await _unitOfWork.StatusRepo.GetAll()
                            .FirstOrDefaultAsync(s => s.Id == existingOrder.StatusId);

                        if (status == null || status.Code != "COMPLETED")
                            return BadRequest(new { success = false, message = "Only COMPLETED orders can be updated." });

                        // Prepare stock validation
                        var insufficientItems = new List<string>();

                        // Map existing items by productId
                        var existingItemsMap = existingOrder.Items.ToDictionary(i => i.ProductId, i => i);

                        foreach (var updatedItem in updatedOrder.Items)
                        {
                            var inventory = updatedItem.Product.Inventories.Where(s => s.StoreId == updatedItem.StoreId).FirstOrDefault(); 
                            if (!existingItemsMap.TryGetValue(updatedItem.ProductId, out var oldItem))
                            {
                                // New item added to invoice
                                if (existingOrder.OrderTypeId != 1 && updatedItem.Quantity > inventory.Quantity)
                                {
                                    insufficientItems.Add($"{updatedItem.Product?.NameAr ?? "Unknown"} (Available: {inventory?.Quantity}, Requested: {updatedItem.Quantity})");
                                }
                            }
                            else
                            {
                                decimal diff = updatedItem.Quantity - oldItem.Quantity;

                                if (existingOrder.OrderTypeId != 1 && diff > 0) // Sales: more qty requested
                                {
                                    if (inventory.Quantity < diff)
                                    {
                                        insufficientItems.Add($"{oldItem.Product?.NameAr ?? "Unknown"} (Available: {inventory?.Quantity}, Extra Requested: {diff})");
                                    }
                                }
                            }
                        }

                        if (insufficientItems.Any())
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new
                            {
                                success = false,
                                message = "Update failed due to insufficient stock for the following items:",
                                insufficientItems
                            });
                        }

                        // Apply changes
                        foreach (var updatedItem in updatedOrder.Items)
                        {
                            var inventory = updatedItem.Product.Inventories.Where(s => s.StoreId == updatedItem.StoreId).FirstOrDefault();
                            
                            if (!existingItemsMap.TryGetValue(updatedItem.ProductId, out var oldItem))
                            {
                                // New item added
                                if (existingOrder.OrderTypeId == 1) // Purchase
                                {
                                    inventory.Quantity += updatedItem.Quantity;
                                }
                                else // Sales
                                {
                                    inventory.Quantity -= updatedItem.Quantity;
                                }

                                await _unitOfWork.ProductRepo.UpdateAsync(updatedItem.Product);

                                var newTransaction = new InventoryTransaction
                                {
                                    ProductId = updatedItem.ProductId,
                                    QuantityChanged = existingOrder.OrderTypeId == 1 ? updatedItem.Quantity : -updatedItem.Quantity,
                                    TransactionDate = DateTime.UtcNow,
                                    Notes = $"Invoice #{existingOrder.Id} updated (new item).",
                                    TransactionTypeId = existingOrder.OrderTypeId == 1 ? 1 : 2, // example lookup
                                    OrderId = existingOrder.Id
                                };
                                await _unitOfWork.InventoryTransactionRepo.CreateAsync(newTransaction);
                            }
                            else
                            {
                                // Existing item modified
                                decimal diff = updatedItem.Quantity - oldItem.Quantity;
                                var inventoryOld = oldItem.Product.Inventories.Where(s => s.StoreId == updatedItem.StoreId).FirstOrDefault();

                                if (diff != 0)
                                {
                                    if (existingOrder.OrderTypeId == 1) // Purchase
                                    {
                                        inventoryOld.Quantity += diff;
                                    }
                                    else // Sales
                                    {
                                        inventoryOld.Quantity -= diff;
                                    }

                                    await _unitOfWork.ProductRepo.UpdateAsync(oldItem.Product);

                                    var updateTransaction = new InventoryTransaction
                                    {
                                        ProductId = oldItem.ProductId,
                                        QuantityChanged = existingOrder.OrderTypeId == 1 ? diff : -diff,
                                        TransactionDate = DateTime.UtcNow,
                                        Notes = $"Invoice #{existingOrder.Id} updated (qty changed).",
                                        TransactionTypeId = existingOrder.OrderTypeId == 1 ? 1 : 2, // example lookup
                                        OrderId = existingOrder.Id
                                    };
                                    await _unitOfWork.InventoryTransactionRepo.CreateAsync(updateTransaction);

                                    oldItem.Quantity = updatedItem.Quantity; // update order item quantity
                                }
                            }
                        }

                        // Save updated order and items
                        existingOrder.Items = updatedOrder.Items;
                        await _unitOfWork.OrderRepo.UpdateAsync(existingOrder);

                        await _unitOfWork.SaveAsync();
                        await transaction.CommitAsync();

                        return Ok(new
                        {
                            success = true,
                            message = $"Invoice #{existingOrder.Id} updated successfully.",
                            result = existingOrder
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new
                        {
                            success = false,
                            message = $"Error updating approved order: {ex.Message}"
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("CancelApprovedOrder/{id}")]
        public async Task<IActionResult> CancelApprovedOrder(int id)
        {
            try
            {
                var strategy = _unitOfWork.GetExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        // Load order with items + products
                        var order = await _unitOfWork.OrderRepo
                            .GetAll()
                            .Include(o => o.Items)
                                .ThenInclude(i => i.Product)
                            .FirstOrDefaultAsync(o => o.Id == id);

                        if (order == null)
                            return NotFound(new { success = false, message = "Order not found." });

                        // Ensure it's already completed
                        var status = await _unitOfWork.StatusRepo.GetAll()
                            .FirstOrDefaultAsync(s => s.Id == order.StatusId);

                        if (status == null || status.Code != "COMPLETED")
                            return BadRequest(new { success = false, message = "Only COMPLETED orders can be cancelled." });

                        // Get Cancel status
                        var cancelStatus = await _unitOfWork.StatusRepo
                            .GetByCondition(s => s.Code == "CANCELLED")
                            .FirstOrDefaultAsync();

                        if (cancelStatus == null)
                            return BadRequest(new { success = false, message = "Cancel status not configured." });

                        // Reverse stock changes
                        foreach (var item in order.Items)
                        {
                            var inventory = item.Product.Inventories.Where(s => s.StoreId == item.StoreId).FirstOrDefault();
                            if (order.OrderTypeId == 1) // Purchase (reverse = subtract stock)
                            {
                                if (inventory.Quantity < item.Quantity)
                                {
                                    await transaction.RollbackAsync();
                                    return BadRequest(new
                                    {
                                        success = false,
                                        message = $"Cannot cancel. Product {item.Product.NameAr} stock would go negative.",
                                        product = item.Product.NameAr,
                                        available = inventory.Quantity,
                                        toRemove = item.Quantity
                                    });
                                }

                                inventory.Quantity -= item.Quantity;
                            }
                            else // Sale (reverse = add stock back)
                            {
                                inventory.Quantity += item.Quantity;
                            }

                            await _unitOfWork.ProductRepo.UpdateAsync(item.Product);

                            // Log reversal transaction
                            var reversalTransaction = new InventoryTransaction
                            {
                                ProductId = item.ProductId,
                                QuantityChanged = order.OrderTypeId == 1 ? -item.Quantity : item.Quantity,
                                TransactionDate = DateTime.UtcNow,
                                Notes = $"Order #{order.Id} cancelled (reversal).",
                                TransactionTypeId = order.OrderTypeId == 1 ? 1 : 2, // adjust lookup accordingly
                                OrderId = order.Id
                            };
                            await _unitOfWork.InventoryTransactionRepo.CreateAsync(reversalTransaction);
                        }

                        // Mark order as cancelled
                        order.StatusId = cancelStatus.Id;
                        await _unitOfWork.OrderRepo.UpdateAsync(order);

                        await _unitOfWork.SaveAsync();
                        await transaction.CommitAsync();

                        return Ok(new
                        {
                            success = true,
                            message = $"Order #{order.Id} has been cancelled and stock changes reverted.",
                            result = order
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, new
                        {
                            success = false,
                            message = $"Error cancelling order: {ex.Message}"
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // New endpoints for variants and modifiers support

        [HttpGet]
        [Route("GetProductVariants/{productId}")]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            try
            {
                var variants = await _unitOfWork.ProductVariantRepo
                    .GetByCondition(v => v.ProductId == productId && v.IsActive)
                    .OrderBy(v => v.DisplayOrder)
                    .ToListAsync();

                return Ok(variants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetProductModifiers/{productId}")]
        public async Task<IActionResult> GetProductModifiers(int productId)
        {
            try
            {
                var modifierGroups = await _unitOfWork.ProductModifierGroupRepo
                    .GetByCondition(mg => mg.ProductId == productId && mg.IsActive)
                    .Include(mg => mg.Modifier)
                        .ThenInclude(m => m.Options)
                    .OrderBy(mg => mg.DisplayOrder)
                    .ToListAsync();

                return Ok(modifierGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("SaveOrderWithVariantsAndModifiers")]
        public async Task<IActionResult> SaveOrderWithVariantsAndModifiers([FromBody] OrderWithVariantsAndModifiersDto orderDto)
        {
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Create the order
                    var order = _mapper.Map<Order>(orderDto);
                    order.CreatedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.OrderRepo.CreateAsync(order);
                    await _unitOfWork.SaveAsync();

                    // Create order items with variants and modifiers
                    foreach (var itemDto in orderDto.Items)
                    {
                        var orderItem = _mapper.Map<OrderItem>(itemDto);
                        orderItem.OrderId = order.Id;
                        orderItem.CreatedAt = DateTime.UtcNow;
                        orderItem.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.OrderItemRepo.CreateAsync(orderItem);
                        await _unitOfWork.SaveAsync();

                        // Create order item modifiers if any
                        if (itemDto.Modifiers != null && itemDto.Modifiers.Any())
                        {
                            foreach (var modifierDto in itemDto.Modifiers)
                            {
                                var orderItemModifier = _mapper.Map<OrderItemModifier>(modifierDto);
                                orderItemModifier.OrderItemId = orderItem.Id;
                                orderItemModifier.CreatedAt = DateTime.UtcNow;
                                orderItemModifier.UpdatedAt = DateTime.UtcNow;

                                await _unitOfWork.OrderItemModifierRepo.CreateAsync(orderItemModifier);
                            }
                        }
                    }

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Order saved successfully with variants and modifiers.",
                        orderId = order.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error saving order: {ex.Message}"
                });
            }
        }

    }
}