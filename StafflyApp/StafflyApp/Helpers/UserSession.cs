namespace StafflyApp.Helpers
{
    public class UserSession
    {
        private static UserSession? _instance;
        public static UserSession Instance => _instance ??= new UserSession();
        private UserSession() { }

        public int UserID { get; set; }
        public string? Username { get; set; }
        public int RoleID { get; set; }
        public string? RoleName { get; set; }

        public void Logout()
        {
            UserID = 0;
            Username = null;
            RoleID = 0;
            RoleName = null;
            _instance = null; // Reset hoàn toàn phiên làm việc
        }
    }
}