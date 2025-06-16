using Microsoft.Extensions.Configuration;

namespace DataMigration
{
    public static class AppConfig
    {
        public static string ConnectionString;

        public static bool Turbo;

        static AppConfig()
        {
            var config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory) // Base path for appsettings.json
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

            // Read connection string
            ConnectionString = config.GetConnectionString("MyDbConnection");

            Turbo = true;
        }
    }
}
