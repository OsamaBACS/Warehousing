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
        
        // Enhanced fields
        public string? Code { get; set; } = string.Empty;        // Unique identifier, like "WH-01", "SHOP-02"
        public string? Address { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public bool IsMainWarehouse { get; set; } = false;       // Flag to identify primary warehouse

        // Audit fields
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }
}