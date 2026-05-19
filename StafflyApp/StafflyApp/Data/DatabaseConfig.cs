using Microsoft.Extensions.Configuration;
using System.IO;

namespace StafflyApp.Data
{
    public class DatabaseConfig
    {
        public static string ConnectionString { get; set; }

        static DatabaseConfig()
        {
            var configuration = new ConfigurationBuilder()
                // SỬA DÒNG NÀY: Để đảm bảo luôn tìm thấy appsettings.json khi chạy App
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }
}