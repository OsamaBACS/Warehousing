using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehousing.Repo.Dtos.Reports;
using Warehousing.Repo.Shared;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("order-summary")]
        public async Task<ActionResult<OrderReportResponseDto>> GetOrderSummary([FromBody] OrderReportFilterDto? filter)
        {
            filter ??= new OrderReportFilterDto();

            var now = DateTime.UtcNow;
            var dateTo = filter.DateTo?.Date.AddDays(1).AddTicks(-1) ?? now;
            var dateFrom = filter.DateFrom?.Date ?? dateTo.Date;

            if (dateFrom > dateTo)
            {
                (dateFrom, dateTo) = (dateTo.Date, dateFrom.Date.AddDays(1).AddTicks(-1));
            }

            var ordersQuery = _unitOfWork.OrderRepo
                .GetAll()
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Store)
                .Include(o => o.Customer)
                .Include(o => o.Supplier)
                .Include(o => o.Status)
                .Include(o => o.OrderType)
                .Where(o => o.OrderDate >= dateFrom && o.OrderDate <= dateTo);

            if (filter.OrderTypeId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderTypeId == filter.OrderTypeId);
            }

            if (filter.CustomerId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CustomerId == filter.CustomerId);
            }

            if (filter.SupplierId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.SupplierId == filter.SupplierId);
            }

            if (filter.StatusId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.StatusId == filter.StatusId);
            }

            if (filter.StoreId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.Items.Any(i => i.StoreId == filter.StoreId));
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalAmount = orders.Sum(o => o.TotalAmount);
            var totalQuantity = orders.Sum(o => o.Items.Sum(i => i.Quantity));

            var summary = new OrderReportSummaryDto
            {
                TotalOrders = orders.Count,
                TotalAmount = totalAmount,
                TotalQuantity = totalQuantity,
                AverageOrderValue = orders.Count > 0 ? totalAmount / orders.Count : 0,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var dailyBreakdown = orders
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new OrderReportDailyDto
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    TotalAmount = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            var maxRecords = filter.MaxRecords <= 0 ? 200 : Math.Min(filter.MaxRecords, 500);

            var detailedOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(maxRecords)
                .Select(o => new OrderReportDetailDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    OrderTypeId = o.OrderTypeId,
                    OrderTypeNameAr = o.OrderType?.NameAr ?? string.Empty,
                    OrderTypeNameEn = o.OrderType?.NameEn ?? o.OrderType?.NameAr ?? string.Empty,
                    CustomerName = o.Customer?.NameAr ?? o.Customer?.NameEn ?? string.Empty,
                    SupplierName = o.Supplier?.Name ?? string.Empty,
                    StatusNameAr = o.Status?.NameAr ?? string.Empty,
                    StatusNameEn = o.Status?.NameEn ?? o.Status?.NameAr ?? string.Empty,
                    TotalAmount = o.TotalAmount,
                    TotalQuantity = o.Items.Sum(i => i.Quantity),
                    Stores = o.Items
                        .GroupBy(i => i.Store)
                        .Select(g => new OrderReportStoreBreakdownDto
                        {
                            StoreId = g.Key.Id,
                            StoreNameAr = g.Key.NameAr,
                            StoreNameEn = g.Key.NameEn,
                            Quantity = g.Sum(i => i.Quantity),
                            TotalAmount = g.Sum(i => (i.UnitPrice * i.Quantity) - i.Discount)
                        })
                        .ToList()
                })
                .ToList();

            var response = new OrderReportResponseDto
            {
                Summary = summary,
                DailyBreakdown = dailyBreakdown,
                Orders = detailedOrders
            };

            return Ok(response);
        }
    }
}

