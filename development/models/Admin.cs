namespace Models.AdminPanel
{
    public partial class Admin
    {
        public Admin()
        {
        }
        public int adminId { get; set; }
        public string adminEmail { get; set; }
        public string adminFullname { get; set; }
        public string adminRole { get; set; }
        public string adminPassword { get; set; }
        public string passwordToken { get; set; }
        public long createdAt { get; set; }
        public long lastLoginAt { get; set; }
        public int? recoveryCode { get; set; }
        public bool deleted { get; set; }
    }
    public class AdminCache
    {
        public int admin_id { get; set; }
        public int inst_id { get; set; }
        public string admin_email { get; set; }
        public string admin_fullname { get; set; }
        public string admin_password { get; set; }
        public string confirm_password { get; set; }
        public string password_token { get; set; }
        public int recovery_code { get; set; }
        public string device_id { get; set; }
    }
}
