namespace Warehousing.Data.Entities
{
    public class RoleCategory : BaseClass
    {
        public int Id { get; set; }
        public Role? Role { get; set; }
        public int RoleId { get; set; }

        public Category? Category { get; set; }
        public int CategoryId { get; set; }
    }
}