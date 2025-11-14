using System.Linq.Expressions;
using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryTransactionController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetTransactionByProductId")]
        public async Task<IActionResult> GetTransactionByProductId(int pageIndex, int pageSize, int id, int storeId = 1)
        {
            try
            {
                Expression<Func<InventoryTransaction, bool>> filter = x =>
                    x.ProductId == id &&
                    (
                        storeId == 0 || // 0 means "All"
                        x.StoreId == storeId
                    );

                var list = await _unitOfWork.InventoryTransactionRepo
                    .GetAllPagination(
                        pageIndex,
                        pageSize,
                        x => x.Id,
                        filter,
                        [p => p.Product, tt => tt.TransactionType, po => po.Order, po => po.Order!.Items, s => s.Store]
                    );

                var TotalSize = await _unitOfWork.InventoryTransactionRepo.GetAll().Where(filter).CountAsync();

                return Ok(new
                {
                    transactions = list,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAllTransactions")]
        public async Task<IActionResult> GetAllTransactions(int pageIndex, int pageSize, int? storeId = null, int? productId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                Expression<Func<InventoryTransaction, bool>> filter = x =>
                    (storeId == null || x.StoreId == storeId) &&
                    (productId == null || x.ProductId == productId) &&
                    (fromDate == null || x.TransactionDate >= fromDate) &&
                    (toDate == null || x.TransactionDate <= toDate);

                var list = await _unitOfWork.InventoryTransactionRepo
                    .GetAllPagination(
                        pageIndex,
                        pageSize,
                        x => x.TransactionDate,
                        filter,
                        [p => p.Product, tt => tt.TransactionType, po => po.Order, s => s.Store]
                    );

                var TotalSize = await _unitOfWork.InventoryTransactionRepo.GetAll().Where(filter).CountAsync();

                return Ok(new
                {
                    transactions = list,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetStockMovementReport")]
        public async Task<IActionResult> GetStockMovementReport(int? storeId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _unitOfWork.InventoryTransactionRepo
                    .GetAll()
                    .Include(t => t.Product)
                    .Include(t => t.Store)
                    .Include(t => t.TransactionType)
                    .AsQueryable();

                if (storeId.HasValue)
                    query = query.Where(t => t.StoreId == storeId.Value);

                if (fromDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= toDate.Value);

                var movements = await query
                    .GroupBy(t => new { t.ProductId, t.Product.NameAr, t.Product.NameEn, t.Product.Code })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductNameAr = g.Key.NameAr,
                        ProductNameEn = g.Key.NameEn,
                        ProductCode = g.Key.Code,
                        TotalIn = g.Where(t => t.QuantityChanged > 0).Sum(t => t.QuantityChanged),
                        TotalOut = g.Where(t => t.QuantityChanged < 0).Sum(t => Math.Abs(t.QuantityChanged)),
                        NetMovement = g.Sum(t => t.QuantityChanged),
                        TransactionCount = g.Count(),
                        LastTransactionDate = g.Max(t => t.TransactionDate)
                    })
                    .OrderByDescending(x => x.TransactionCount)
                    .ToListAsync();

                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetInventoryValuationReport")]
        public async Task<IActionResult> GetInventoryValuationReport(int? storeId = null)
        {
            try
            {
                var query = _unitOfWork.InventoryRepo
                    .GetAll()
                    .Include(i => i.Product)
                    .Include(i => i.Store)
                    .AsQueryable();

                if (storeId.HasValue)
                    query = query.Where(i => i.StoreId == storeId.Value);

                var valuations = await query
                    .Select(i => new
                    {
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductCode = i.Product.Code,
                        StoreId = i.StoreId,
                        StoreNameAr = i.Store.NameAr,
                        StoreNameEn = i.Store.NameEn,
                        Quantity = i.Quantity,
                        UnitCost = i.Product.CostPrice,
                        TotalCost = i.Quantity * i.Product.CostPrice,
                        UnitSellingPrice = i.Product.SellingPrice,
                        TotalValue = i.Quantity * i.Product.SellingPrice,
                        LastUpdated = i.UpdatedAt ?? i.CreatedAt
                    })
                    .OrderBy(x => x.StoreNameAr)
                    .ThenBy(x => x.ProductNameAr)
                    .ToListAsync();

                var summary = new
                {
                    TotalItems = valuations.Count,
                    TotalQuantity = valuations.Sum(v => v.Quantity),
                    TotalCostValue = valuations.Sum(v => v.TotalCost),
                    TotalSellingValue = valuations.Sum(v => v.TotalValue),
                    ItemsByStore = valuations.GroupBy(v => v.StoreNameAr)
                        .Select(g => new
                        {
                            StoreName = g.Key,
                            ItemCount = g.Count(),
                            TotalQuantity = g.Sum(x => x.Quantity),
                            TotalCostValue = g.Sum(x => x.TotalCost),
                            TotalSellingValue = g.Sum(x => x.TotalValue)
                        })
                };

                return Ok(new { valuations, summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetLowStockReport")]
        public async Task<IActionResult> GetLowStockReport(decimal threshold = 10, int? storeId = null)
        {
            try
            {
                var query = _unitOfWork.InventoryRepo
                    .GetAll()
                    .Include(i => i.Product)
                    .Include(i => i.Store)
                    .AsQueryable();

                if (storeId.HasValue)
                    query = query.Where(i => i.StoreId == storeId.Value);

                var lowStockItems = await query
                    .Where(i => i.Quantity <= threshold)
                    .Select(i => new
                    {
                        ProductId = i.ProductId,
                        ProductNameAr = i.Product.NameAr,
                        ProductNameEn = i.Product.NameEn,
                        ProductCode = i.Product.Code,
                        StoreId = i.StoreId,
                        StoreNameAr = i.Store.NameAr,
                        StoreNameEn = i.Store.NameEn,
                        CurrentQuantity = i.Quantity,
                        ReorderLevel = 10, // Default reorder level since it's not in Product entity
                        IsBelowReorderLevel = i.Quantity <= 10,
                        UnitCost = i.Product.CostPrice,
                        UnitSellingPrice = i.Product.SellingPrice,
                        LastUpdated = i.UpdatedAt ?? i.CreatedAt
                    })
                    .OrderBy(x => x.CurrentQuantity)
                    .ToListAsync();

                var summary = new
                {
                    TotalLowStockItems = lowStockItems.Count(),
                    BelowReorderLevel = lowStockItems.Count(i => i.IsBelowReorderLevel),
                    ZeroStockItems = lowStockItems.Count(i => i.CurrentQuantity == 0),
                    TotalEstimatedValue = lowStockItems.Sum(i => i.CurrentQuantity * i.UnitCost)
                };

                return Ok(new { lowStockItems, summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTransactionTrends")]
        public async Task<IActionResult> GetTransactionTrends(int? storeId = null, int? productId = null, int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);
                
                var query = _unitOfWork.InventoryTransactionRepo
                    .GetAll()
                    .Include(t => t.Product)
                    .Include(t => t.Store)
                    .Include(t => t.TransactionType)
                    .Where(t => t.TransactionDate >= startDate)
                    .AsQueryable();

                if (storeId.HasValue)
                    query = query.Where(t => t.StoreId == storeId.Value);

                if (productId.HasValue)
                    query = query.Where(t => t.ProductId == productId.Value);

                var trends = await query
                    .GroupBy(t => new { 
                        Year = t.TransactionDate.Year, 
                        Month = t.TransactionDate.Month,
                        TransactionType = t.TransactionType.NameAr
                    })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TransactionType = g.Key.TransactionType,
                        TransactionCount = g.Count(),
                        TotalQuantity = g.Sum(t => Math.Abs(t.QuantityChanged)),
                        TotalValue = g.Sum(t => Math.Abs(t.QuantityChanged) * t.UnitCost)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                var monthlySummary = trends
                    .GroupBy(t => new { t.Year, t.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalTransactions = g.Sum(x => x.TransactionCount),
                        TotalQuantity = g.Sum(x => x.TotalQuantity),
                        TotalValue = g.Sum(x => x.TotalValue),
                        TransactionTypes = g.Select(x => new { x.TransactionType, x.TransactionCount, x.TotalQuantity, x.TotalValue })
                    })
                    .ToList();

                return Ok(monthlySummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTopMovingProducts")]
        public async Task<IActionResult> GetTopMovingProducts(int? storeId = null, DateTime? fromDate = null, DateTime? toDate = null, int topCount = 10)
        {
            try
            {
                var query = _unitOfWork.InventoryTransactionRepo
                    .GetAll()
                    .Include(t => t.Product)
                    .Include(t => t.Store)
                    .Include(t => t.TransactionType)
                    .AsQueryable();

                if (storeId.HasValue)
                    query = query.Where(t => t.StoreId == storeId.Value);

                if (fromDate.HasValue)
                    query = query.Where(t => t.TransactionDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(t => t.TransactionDate <= toDate.Value);

                var topProducts = await query
                    .GroupBy(t => new { t.ProductId, t.Product.NameAr, t.Product.NameEn, t.Product.Code })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        ProductNameAr = g.Key.NameAr,
                        ProductNameEn = g.Key.NameEn,
                        ProductCode = g.Key.Code,
                        TotalTransactions = g.Count(),
                        TotalQuantityMoved = g.Sum(t => Math.Abs(t.QuantityChanged)),
                        TotalIn = g.Where(t => t.QuantityChanged > 0).Sum(t => t.QuantityChanged),
                        TotalOut = g.Where(t => t.QuantityChanged < 0).Sum(t => Math.Abs(t.QuantityChanged)),
                        NetMovement = g.Sum(t => t.QuantityChanged),
                        TotalValue = g.Sum(t => Math.Abs(t.QuantityChanged) * t.UnitCost),
                        LastTransactionDate = g.Max(t => t.TransactionDate)
                    })
                    .OrderByDescending(x => x.TotalQuantityMoved)
                    .Take(topCount)
                    .ToListAsync();

                return Ok(topProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}