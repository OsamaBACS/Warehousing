using System.Collections.Generic;

namespace Warehousing.Repo.Dtos.Reports
{
    public class OrderReportResponseDto
    {
        public OrderReportSummaryDto Summary { get; set; } = new();
        public IEnumerable<OrderReportDailyDto> DailyBreakdown { get; set; } = new List<OrderReportDailyDto>();
        public IEnumerable<OrderReportDetailDto> Orders { get; set; } = new List<OrderReportDetailDto>();
    }
}

