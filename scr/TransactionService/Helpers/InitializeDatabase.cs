using Microsoft.EntityFrameworkCore;
using TransactionService.Data;

namespace TransactionService.Helpers
{
    public static class InitializeDatabase
    {
        public static async Task InitDbAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

            // DD Migrations
            await MigrateDbContextAsync(dbContext);

            await dbContext.SaveChangesAsync();
        }

        public static async Task DropIdentityDatabasesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

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
