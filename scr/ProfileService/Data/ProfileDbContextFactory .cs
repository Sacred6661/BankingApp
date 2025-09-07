using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ProfileService.Data
{
    public class ProfileDbContextFactory : IDesignTimeDbContextFactory<ProfileDbContext>
    {
        public ProfileDbContext CreateDbContext(string[] args)
        {
            // Створюємо конфігурацію
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json") // або інший файл конфігурації
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ProfileDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString); // або UseSqlServer / UseSqlite тощо

            return new ProfileDbContext(optionsBuilder.Options);
        }
    }
}
