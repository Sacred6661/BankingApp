using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nager.Country;
using NodaTime;
using NodaTime.Extensions;
using ProfileService.Data;
using ProfileService.Data.Models;
using System.Globalization;

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
                    Id = (int)ContactTypeEnum.PrimaryEmail,
                    TypeName = "Primary Email",
                    TypeDescription = "Primry Email address that cann't be changed"
                },
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

        public static async Task SeedLanguagesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            var languages = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
               .Select(c => new Language
               {
                   Code = c.TwoLetterISOLanguageName,    // "en", "uk", "de"
                   Name = c.EnglishName                  // always English
               })
               .GroupBy(l => l.Code)                     // unique code
               .Select(g => g.First())
               .OrderBy(l => l.Name)
               .ToList();

            dbContext.Languages.AddRange(languages);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedTimezonesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            var now = SystemClock.Instance.GetCurrentInstant();

            var timeZones = DateTimeZoneProviders.Tzdb
                .GetAllZones()
                .Select(tz =>
                {
                    var offset = tz.GetUtcOffset(now);          // NodaTime.Offset
                    var ts = offset.ToTimeSpan();               // Convert to TimeSpan
                    var sign = ts.TotalMinutes >= 0 ? "+" : "-";
                    var hours = Math.Abs(ts.Hours).ToString("D2");
                    var minutes = Math.Abs(ts.Minutes).ToString("D2");
                    var formattedOffset = $"{sign}{hours}:{minutes}";

                    return new Timezone
                    {
                        Code = tz.Id,
                        Name = $"(UTC {formattedOffset}) {tz.Id}",
                        UtcOffset = formattedOffset,
                        OffsetMinutes = (int)ts.TotalMinutes         // for sorting
                    };
                })
                .OrderBy(t => t.OffsetMinutes)                       // from biggest to smallest one
                .ToList();

            // just in case
            dbContext.Timezones.RemoveRange(dbContext.Timezones);   // clear old data
            dbContext.Timezones.AddRange(timeZones);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedCountriesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

            if (dbContext.Countries.Any()) return; // щоб не дублювати

            var countryProvider = new CountryProvider();
            var countries = countryProvider.GetCountries();

            var countryEntities = countries.Select(c => new Country
            {
                Alpha2Code = c.Alpha2Code.ToString(),
                Alpha3Code = c.Alpha3Code.ToString(),
                NumericCode = c.NumericCode,
                Name = c.CommonName
            }).ToList();

            dbContext.Countries.AddRange(countryEntities);
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
