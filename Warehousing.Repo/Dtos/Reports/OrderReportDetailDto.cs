using System;
using System.Collections.Generic;

namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportDetailDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int? OrderTypeId { get; set; }
        public string OrderTypeNameAr { get; set; } = string.Empty;
        public string? OrderTypeNameEn { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;
        public string? StatusNameEn { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public IEnumerable<OrderReportStoreBreakdownDto> Stores { get; set; } = new List<OrderReportStoreBreakdownDto>();
    }
}

