using AuthServer.Data;
using MassTransit;
using Messaging;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Consumers
{
    public class ProfileCompletedConsumer : IConsumer<ProfileCompleted>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileCompletedConsumer(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Consume(ConsumeContext<ProfileCompleted> context)
        {
            var msg = context.Message;
            var user = await _userManager.FindByIdAsync(msg.UserId);

            if (user != null)
            {
                user.IsProfileComplete = true;
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
