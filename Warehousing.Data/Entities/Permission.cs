namespace Warehousing.Data.Entities
{
    public class Permission : BaseClass
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty; // e.g. "ViewProducts"
        public string NameAr { get; set; } = string.Empty;     
        public string Code { get; set; } = string.Empty;     // e.g. "PERM_VIEW_PRODUCTS"
    }
}