using System;

namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}

