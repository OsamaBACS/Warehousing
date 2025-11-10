namespace Warehousing.Data.Entities
{
    public class RoleSubCategory : BaseClass
    {
        public int Id { get; set; }
        public Role? Role { get; set; }
        public int RoleId { get; set; }

        public SubCategory? SubCategory { get; set; }
        public int SubCategoryId { get; set; }
    }
}






