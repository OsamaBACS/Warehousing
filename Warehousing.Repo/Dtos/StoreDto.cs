using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warehousing.Repo.Dtos
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string? NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}