using Microsoft.EntityFrameworkCore;
using ProfileService.Data.Models;

namespace ProfileService.Data
{

    public class ProfileDbContext : DbContext
    {
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<ProfileContact> ProfileContacts { get; set; } = null!;
        public DbSet<ProfileAddress> ProfileAddresses { get; set; } = null!;
        public DbSet<ProfileSettings> ProfileSettings { get; set; } = null!;
        public DbSet<AddressType> AddressTypes { get; set; } = null!;
        public DbSet<ContactType> ContactTypes { get; set; } = null!;
        public DbSet<Language> Languages { get; set; } = null;
        public DbSet<Timezone> Timezones { get; set; } = null;
        public DbSet<Country> Countries { get; set; } = null;
    }
}
