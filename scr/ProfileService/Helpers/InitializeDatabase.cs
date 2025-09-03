using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Data.Models;

namespace ProfileService.Helpers
{
    public static class InitializeDatabase
    {
        public static async Task InitDbAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            // DD Migrations
            await MigrateDbContextAsync(dbContext);

            await dbContext.SaveChangesAsync();
        }

        public static async Task DropDatabasesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            await DropDbContextAsync(dbContext);
        }

        public static async Task SeedAddressTypesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            var addressTypes = new List<AddressType>
            {
                new AddressType
                {
                    Id = (int)AddressTypeEnum.Home,
                    TypeName = "Home",
                    TypeDescription = "Home address info"
                },
                new AddressType
                {
                    Id = (int)AddressTypeEnum.Work,
                    TypeName = "Work",
                    TypeDescription = "Work address info"
                },
                new AddressType
                {
                    Id = (int)AddressTypeEnum.Billing,
                    TypeName = "Billing",
                    TypeDescription = "Billing address info"
                }
            };

            dbContext.AddressTypes.AddRange(addressTypes);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedContactTypesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            var addressTypes = new List<ContactType>
            {
                new ContactType
                {
                    Id = (int)ContactTypeEnum.Email,
                    TypeName = "Email",
                    TypeDescription = "Email address"
                },
                new ContactType
                {
                    Id = (int)ContactTypeEnum.Phone,
                    TypeName = "Phone",
                    TypeDescription = "Phone number"
                },
                new ContactType
                {
                    Id = (int)ContactTypeEnum.Telegram,
                    TypeName = "Telegram",
                    TypeDescription = "Telegram username"
                }
            };

            dbContext.ContactTypes.AddRange(addressTypes);
            await dbContext.SaveChangesAsync();
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
