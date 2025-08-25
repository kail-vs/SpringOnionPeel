using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpringOnion.Data;
using SpringOnion.Data.Repositories;
using SpringOnion.Services;
using SpringOnion.ViewModels;
using SpringOnion.Views;

namespace SpringOnion
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            var configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            builder.Configuration.AddInMemoryCollection(configData!);

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "springonion.db");
            var connStr = $"Data Source={dbPath}";

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<Dashboard>();

            builder.UseMauiApp<App>().UseMauiCommunityToolkit();

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlite(connStr);
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            });

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<UserSyncService>();

            return builder.Build();
        }
    }
}
