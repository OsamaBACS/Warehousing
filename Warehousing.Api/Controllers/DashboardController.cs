using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<object>> GetDashboardOverview()
        {
            try
            {
                var overview = new
                {
                    // Inventory Summary
                    TotalProducts = await _unitOfWork.ProductRepo.GetByCondition(p => p.IsActive).CountAsync(),
                    TotalStores = await _unitOfWork.StoreRepo.GetByCondition(s => s.IsActive).CountAsync(),
                    TotalInventoryQuantity = await _unitOfWork.InventoryRepo.GetAll().SumAsync(i => i.Quantity),
                    
                    // Low Stock Alerts
                    LowStockProducts = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.Quantity <= 10 && i.Quantity > 0)
                        .CountAsync(),
                    ZeroStockProducts = await _unitOfWork.InventoryRepo
                        .GetByCondition(i => i.Quantity == 0)
                        .CountAsync(),
                    
                    // Recent Activity
                    RecentOrders = await _unitOfWork.OrderRepo
                        .GetByCondition(o => o.OrderDate >= DateTime.UtcNow.AddDays(-7))
                        .CountAsync(),
                    
                    RecentTransfers = await _unitOfWork.StoreTransferRepo
                        .GetAll()
                        .Where(t => t.TransferDate >= DateTime.UtcNow.AddDays(-7))
                        .CountAsync()
                };

                return Ok(overview);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("recent-transactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentTransactions([FromQuery] int count = 10)
        {
            try
            {
                var transactions = await _unitOfWork.InventoryTransactionRepo
                    .GetAll()
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(count)
                    .Select(t => new
                    {
                        Id = t.Id,
                        ProductNameAr = t.Product.NameAr,
                        ProductNameEn = t.Product.NameEn,
                        StoreNameAr = t.Store.NameAr,
                        StoreNameEn = t.Store.NameEn,
                        TransactionTypeNameAr = t.TransactionType.NameAr,
                        TransactionTypeNameEn = t.TransactionType.NameEn,
                        QuantityChanged = t.QuantityChanged,
                        TransactionDate = t.TransactionDate,
                        Notes = t.Notes
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-products")]
        public async Task<ActionResult<IEnumerable<object>>> GetTopProducts([FromQuery] int count = 10)
        {
            try
            {
                var topProducts = await _unitOfWork.InventoryRepo
                    .GetAll()
                    .GroupBy(i => i.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        ProductNameAr = g.First().Product.NameAr,
                        ProductNameEn = g.First().Product.NameEn,
                        ProductCode = g.First().Product.Code,
                        TotalQuantity = g.Sum(i => i.Quantity),
                        StoreCount = g.Count()
                    })
                    .OrderByDescending(p => p.TotalQuantity)
                    .Take(count)
                    .ToListAsync();

                return Ok(topProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("store-performance")]
        public async Task<ActionResult<IEnumerable<object>>> GetStorePerformance()
        {
            try
            {
                var storePerformance = await _unitOfWork.StoreRepo
                    .GetByCondition(s => s.IsActive)
                    .Select(s => new
                    {
                        StoreId = s.Id,
                        StoreNameAr = s.NameAr,
                        StoreNameEn = s.NameEn,
                        StoreCode = s.Code,
                        IsMainWarehouse = s.IsMainWarehouse,
                        TotalProducts = s.Inventories.Count,
                        TotalQuantity = s.Inventories.Sum(i => i.Quantity),
                        LowStockProducts = s.Inventories.Count(i => i.Quantity <= 10 && i.Quantity > 0),
                        ZeroStockProducts = s.Inventories.Count(i => i.Quantity == 0)
                    })
                    .ToListAsync();

                return Ok(storePerformance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("monthly-transactions")]
        public async Task<ActionResult<IEnumerable<object>>> GetMonthlyTransactions([FromQuery] int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var monthlyTransactions = await _unitOfWork.InventoryTransactionRepo
                    .GetByCondition(t => t.TransactionDate >= startDate)
                    .GroupBy(t => new { 
                        Year = t.TransactionDate.Year, 
                        Month = t.TransactionDate.Month 
                    })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TransactionCount = g.Count(),
                        TotalQuantityChanged = g.Sum(t => Math.Abs(t.QuantityChanged)),
                        PurchaseTransactions = g.Count(t => t.TransactionType.Code == "PURCHASE"),
                        SaleTransactions = g.Count(t => t.TransactionType.Code == "SALE"),
                        AdjustmentTransactions = g.Count(t => t.TransactionType.Code.StartsWith("ADJUSTMENT"))
                    })
                    .OrderBy(t => t.Year)
                    .ThenBy(t => t.Month)
                    .ToListAsync();

                return Ok(monthlyTransactions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("alerts")]
        public async Task<ActionResult<IEnumerable<object>>> GetAlerts()
        {
            try
            {
                var alerts = new List<object>();

                // Low stock alerts
                var lowStockItems = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.Quantity <= 10 && i.Quantity > 0)
                    .Select(i => new
                    {
                        Type = "Low Stock",
                        Severity = i.Quantity <= 5 ? "High" : "Medium",
                        Message = $"{i.Product.NameAr} is running low in {i.Store.NameAr} (Quantity: {i.Quantity})",
                        ProductId = i.ProductId,
                        StoreId = i.StoreId,
                        CurrentQuantity = i.Quantity
                    })
                    .Take(10)
                    .ToListAsync();

                alerts.AddRange(lowStockItems);

                // Zero stock alerts
                var zeroStockItems = await _unitOfWork.InventoryRepo
                    .GetByCondition(i => i.Quantity == 0)
                    .Select(i => new
                    {
                        Type = "Out of Stock",
                        Severity = "High",
                        Message = $"{i.Product.NameAr} is out of stock in {i.Store.NameAr}",
                        ProductId = i.ProductId,
                        StoreId = i.StoreId,
                        CurrentQuantity = i.Quantity
                    })
                    .Take(10)
                    .ToListAsync();

                alerts.AddRange(zeroStockItems);

                return Ok(alerts.OrderBy(a => ((dynamic)a).Severity == "High" ? 1 : 2));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}



