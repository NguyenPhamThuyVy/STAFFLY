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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }
}