namespace Warehousing.Data.Entities
{
    public class RoleProduct : BaseClass
    {
        public int Id { get; set; }
        public Role? Role { get; set; }
        public int RoleId { get; set; }

        public Product? Product { get; set; }
        public int ProductId { get; set; }
    }
}