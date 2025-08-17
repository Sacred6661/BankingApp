using AuthServer.Config;
using AuthServer.Data.Configuration;
using AuthServer.Data.PersistedGrants;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.Mappers;
using AuthServer.Helpers;
using AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Services;
using AuthServer.Services;
using Common.Logging;

var builder = WebApplication.CreateBuilder(args);

LoggingSetup.ConfigureLogging(builder);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Config.Init(builder.Configuration);

// IdentityServer Configuration
builder.Services.AddDbContext<CustomConfigurationDbContext>(options =>
    options.UseNpgsql(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(CustomConfigurationDbContext).Assembly.GetName().Name);
    }));

// DbContext context for Persisted Grants
builder.Services.AddDbContext<CustomPersistedGrantDbContext>(options =>
    options.UseNpgsql(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(CustomPersistedGrantDbContext).Assembly.GetName().Name);
    }));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
    }));

// Identity Add
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// IdentityServer with EF Stores
builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore<CustomConfigurationDbContext>(options =>
    {
        options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
            sql => sql.MigrationsAssembly(typeof(CustomConfigurationDbContext).Assembly.GetName().Name));
    })
    .AddOperationalStore<CustomPersistedGrantDbContext>(options =>
    {
        options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
            sql => sql.MigrationsAssembly(typeof(CustomPersistedGrantDbContext).Assembly.GetName().Name));
    })
    .AddDeveloperSigningCredential();  // certificate for dev

builder.Services.AddTransient<IProfileService, CustomProfileService>();

builder.Services.AddControllers();

var app = builder.Build();

LoggingSetup.UseCorrelationLogging(app);

app.UseIdentityServer();

app.MapControllers();

// DropIdentityDatabasesAsync use only if you want always clear DB before initialization
await InitializeDatabase.DropIdentityDatabasesAsync(app.Services);
await InitializeDatabase.InitDbAsync(app.Services);
await InitializeDatabase.SeedUsersAsync(app.Services);

app.Run();
