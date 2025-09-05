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

                if (!isAdmin)
                {
                    result = result.Where(o => o.UserId == UserId);
                }

                if (filter.OrderDate != null)
                {
                    result = result.Where(o => o.OrderDate == filter.OrderDate);
                }

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
                            item.CostPrice = itemDto.CostPrice;
                            item.SellingPrice = item.SellingPrice;
                        }
                        else
                        {
                            // Add new item
                            order.Items.Add(new OrderItem
                            {
                                StoreId = itemDto.StoreId,
                                ProductId = itemDto.ProductId,
                                Quantity = itemDto.Quantity,
                                CostPrice = itemDto.CostPrice,
                                SellingPrice = itemDto.SellingPrice,
                                OrderId = dto.Id
                            });
                        }
                    }

                    var result = await _unitOfWork.OrderRepo.UpdateAsync(order);
                    return Ok(result);
                }
                else
                {
                    var entity = new Order
                    {
                        OrderDate = dto.OrderDate,
                        TotalAmount = dto.TotalAmount,
                        OrderTypeId = dto.OrderTypeId,
                        CustomerId = dto.CustomerId,
                        SupplierId = dto.SupplierId,
                        StatusId = dto.StatusId,
                        Items = dto.Items.Select(i => new OrderItem
                        {
                            StoreId = i.StoreId,
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            CostPrice = i.CostPrice,
                            SellingPrice = i.SellingPrice
                        }).ToList()
                    };

                    var result = await _unitOfWork.OrderRepo.CreateAsync(entity);
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
                            if (order.OrderTypeId != 1 && item.Product.QuantityInStock < item.Quantity)
                            {
                                insufficientItems.Add($"{item.Product.NameAr} (Available: {item.Product.QuantityInStock}, Requested: {item.Quantity})");
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
                            if (order.OrderTypeId == 1) // Purchase → Stock In
                            {
                                item.Product.QuantityInStock += item.Quantity;
                            }
                            else // Sale → Stock Out (already validated)
                            {
                                item.Product.QuantityInStock -= item.Quantity;
                            }

                            await _unitOfWork.ProductRepo.UpdateAsync(item.Product);

                            var transactionEntity = new InventoryTransaction
                            {
                                ProductId = item.ProductId,
                                QuantityChanged = order.OrderTypeId == 1 ? item.Quantity : -item.Quantity,
                                TransactionDate = DateTime.UtcNow,
                                Notes = $"{transactionTypeCode} order #{order.Id} completed.",
                                TransactionTypeId = transactionType.Id,
                                OrderId = order.Id
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
                            if (!existingItemsMap.TryGetValue(updatedItem.ProductId, out var oldItem))
                            {
                                // New item added to invoice
                                if (existingOrder.OrderTypeId != 1 && updatedItem.Quantity > updatedItem.Product.QuantityInStock)
                                {
                                    insufficientItems.Add($"{updatedItem.Product?.NameAr ?? "Unknown"} (Available: {updatedItem.Product?.QuantityInStock}, Requested: {updatedItem.Quantity})");
                                }
                            }
                            else
                            {
                                decimal diff = updatedItem.Quantity - oldItem.Quantity;

                                if (existingOrder.OrderTypeId != 1 && diff > 0) // Sales: more qty requested
                                {
                                    if (oldItem.Product.QuantityInStock < diff)
                                    {
                                        insufficientItems.Add($"{oldItem.Product?.NameAr ?? "Unknown"} (Available: {oldItem.Product?.QuantityInStock}, Extra Requested: {diff})");
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
                            if (!existingItemsMap.TryGetValue(updatedItem.ProductId, out var oldItem))
                            {
                                // New item added
                                if (existingOrder.OrderTypeId == 1) // Purchase
                                {
                                    updatedItem.Product.QuantityInStock += updatedItem.Quantity;
                                }
                                else // Sales
                                {
                                    updatedItem.Product.QuantityInStock -= updatedItem.Quantity;
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

                                if (diff != 0)
                                {
                                    if (existingOrder.OrderTypeId == 1) // Purchase
                                    {
                                        oldItem.Product.QuantityInStock += diff;
                                    }
                                    else // Sales
                                    {
                                        oldItem.Product.QuantityInStock -= diff;
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

    }
}