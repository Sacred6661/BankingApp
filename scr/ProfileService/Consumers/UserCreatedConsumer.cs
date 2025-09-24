using MassTransit;
using Messaging;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Data.Models;
using System.Diagnostics;

namespace ProfileService.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly ProfileDbContext _dbContext;

        public UserCreatedConsumer(ProfileDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var msg = context.Message;

            var userId = Guid.Parse(msg.UserId);
            var email = msg.Email;

            var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            try
            {
                if (profile == null)
                {
                    var addProfile = new Profile
                    {
                        UserId = userId,
                        Email = email,
                        Contacts = new List<ProfileContact>(),
                        Settings = new ProfileSettings()
                    };

                    addProfile.Contacts.Add(new ProfileContact
                    {
                        UserId = userId,
                        ContactTypeEnum = ContactTypeEnum.PrimaryEmail,
                        Value = email
                    });

                    addProfile.Settings = new ProfileSettings
                    {
                        UserId = userId
                    };

                    _dbContext.Profiles.Add(addProfile);
                    await _dbContext.SaveChangesAsync();


                    _dbContext.ChangeTracker.DetectChanges();
                    foreach (var entry in _dbContext.ChangeTracker.Entries())
                    {
                        Debug.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
