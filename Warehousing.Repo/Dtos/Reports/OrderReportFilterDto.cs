using System;

namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportFilterDto
    {
        public int? OrderTypeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? StoreId { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? StatusId { get; set; }
        public int MaxRecords { get; set; } = 200;
    }
}

