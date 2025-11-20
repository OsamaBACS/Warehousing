using System;

namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportDailyDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

