using AccountService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Helpers
{
    public static class InitializeDatabase
    {
        public static async Task InitDbAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

            // DD Migrations
            await MigrateDbContextAsync(dbContext);

            await dbContext.SaveChangesAsync();
        }

        public static async Task DropIdentityDatabasesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

            await DropDbContextAsync(dbContext);
        }

        private static async Task MigrateDbContextAsync(DbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();
        }

        private static async Task DropDbContextAsync(DbContext dbContext)
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }
}
