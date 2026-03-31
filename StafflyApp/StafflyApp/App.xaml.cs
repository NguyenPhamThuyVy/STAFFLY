using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StafflyApp.Data;
using StafflyApp.Data.Interfaces;
using StafflyApp.Data.Repositories;
using StafflyApp.ViewModels;
using StafflyApp.Views;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace StafflyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public App()
        {
            // 1. Khởi tạo Configuration để đọc file appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // 2. Thiết lập ServiceCollection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            // 3. Lấy Connection String từ file appsettings.json
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            // 4. Đăng ký DbContext với chuỗi kết nối vừa lấy được
            services.AddDbContext<StafflyDbContext>(options =>
                options.UseSqlServer(connectionString));
            // 5. Đăng ký ViewModels
            // services.AddTransient<LoginViewModel>();
            services.AddTransient<EmployeeViewModel>();
            // 6. Đăng ký Repository
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            // 7. Đăng ký Services

            // 8. Đăng ký Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Yêu cầu ServiceProvider lấy ra thực thể LoginWindow đã được cấu hình DI
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }
    }

}
