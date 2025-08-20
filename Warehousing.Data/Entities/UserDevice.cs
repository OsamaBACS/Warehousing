namespace Warehousing.Data.Entities
{
    public class UserDevice
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Fingerprint { get; set; }
        public string IPAddress { get; set; }
        public bool IsApproved { get; set; }
        public DateTime FirstSeen { get; set; }
    }
}