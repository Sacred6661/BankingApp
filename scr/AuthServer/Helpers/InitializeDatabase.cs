using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.Mappers;
using AuthServer.Data.Configuration;
using AuthServer.Data.PersistedGrants;
using AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using static System.Formats.Asn1.AsnWriter;
using System.Data;

namespace AuthServer.Helpers
{
    public static class InitializeDatabase
    {
        public static async Task InitDbAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var configurationDbContext = scope.ServiceProvider.GetRequiredService<CustomConfigurationDbContext>();
            var persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<CustomPersistedGrantDbContext>();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // DD Migrations
            await MigrateDbContextAsync(applicationDbContext);
            await MigrateDbContextAsync(configurationDbContext);
            await MigrateDbContextAsync(persistedGrantDbContext);

            // Clients and Resources adding
            AddClientsIfNotExist(configurationDbContext);
            AddApiScopesIfNotExist(configurationDbContext);
            AddApiResourcesIfNotExist(configurationDbContext);
            AddIdentityResourcesIfNotExist(configurationDbContext);

            await configurationDbContext.SaveChangesAsync();
        }

        public static async Task SeedUsersAsync(IServiceProvider services)
        {
            await SeedUserRolesAsync(services);
            await AddAdminAsync(services, "admin@bank.com", "Admin123!");
        }

        public static async Task SeedUserRolesAsync(IServiceProvider services)
        {
            string[] roles = ["Admin", "User"];

            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

        }

        public static async Task DropIdentityDatabasesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var configurationDbContext = scope.ServiceProvider.GetRequiredService<CustomConfigurationDbContext>();
            var persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<CustomPersistedGrantDbContext>();
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await DropDbContextAsync(configurationDbContext);
            await DropDbContextAsync(persistedGrantDbContext);
            await DropDbContextAsync(applicationDbContext);
        }

        public static async Task AddAdminAsync(IServiceProvider services, string email, string password)
        {
            using var scope = services.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };
                await userManager.CreateAsync(user, password);
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }



        private static async Task MigrateDbContextAsync(DbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();
        }

        private static async Task DropDbContextAsync(DbContext dbContext)
        {
            await dbContext.Database.EnsureDeletedAsync();
        }

        private static void AddClientsIfNotExist(CustomConfigurationDbContext dbContext)
        {
            foreach (var client in AuthServer.Config.Config.Clients)
                if (!dbContext.Clients.Any(c => c.ClientId == client.ClientId))
                    dbContext.Clients.Add(client.ToEntity());
        }

        private static void AddApiScopesIfNotExist(CustomConfigurationDbContext dbContext)
        {
            foreach (var apiScope in AuthServer.Config.Config.ApiScopes)
            {
                if (!dbContext.ApiScopes.Any(s => s.Name == apiScope.Name))
                    dbContext.ApiScopes.Add(apiScope.ToEntity());
            }
        }

        private static void AddApiResourcesIfNotExist(CustomConfigurationDbContext dbContext)
        {
            foreach (var apiResource in AuthServer.Config.Config.ApiResources)
            {
                if (!dbContext.ApiResources.Any(r => r.Name == apiResource.Name))
                {
                    dbContext.ApiResources.Add(apiResource.ToEntity());
                }
            }
        }

        private static void AddIdentityResourcesIfNotExist(CustomConfigurationDbContext dbContext)
        {
            foreach (var identityResource in AuthServer.Config.Config.IdentityResources)
            {
                if (!dbContext.IdentityResources.Any(r => r.Name == identityResource.Name))
                    dbContext.IdentityResources.Add(identityResource.ToEntity());
            }
        }
    }
}
