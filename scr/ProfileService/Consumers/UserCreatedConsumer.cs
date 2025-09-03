using MassTransit;
using Messaging;
using ProfileService.Data;
using ProfileService.Data.Models;

namespace ProfileService.Consumers
{
    public class UserCreatedConsumer(ProfileDbContext dbContext) : IConsumer<UserCreated>
    {
        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var msg = context.Message;

            var userId = Guid.Parse(msg.UserId);
            var email = msg.Email;

            var profile = dbContext.Profiles.Where(p => p.UserId == userId)?.FirstOrDefault();
            if(profile == null)
            {
                var addProfile = new Profile
                {
                    UserId = userId
                };

                var addEmailContact = new ProfileContact
                {
                    UserId = userId,
                    ContactTypeEnum = ContactTypeEnum.Email,
                    Value = email
                };

                var addProfileSettings = new ProfileSettings()
                {
                    UserId = userId
                };

                // adding profile and related tables data (email from registration and default settings)
                dbContext.Profiles.Add(addProfile);
                dbContext.ProfileContacts.Add(addEmailContact);
                dbContext.ProfileSettings.Add(addProfileSettings);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
