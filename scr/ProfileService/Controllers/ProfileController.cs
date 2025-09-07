using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using ProfileService.Data;
using ProfileService.Data.Models;
using ProfileService.DTOs;
using ProfileService.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProfileService.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class ProfileController(ProfileDbContext dbContext, IMapper mapper, ILogger<ProfileController> logger) : ControllerBase
    {
        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:guid}")]
        public async Task<IActionResult> GetProfile(Guid? userId)
        {
            var profile = await dbContext.Profiles
                .Include(p => p.Contacts).ThenInclude(c => c.ContactType)
                .Include(p => p.Addresses).ThenInclude(a => a.AddressType)
                .Include(p => p.Settings)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Profile not found",
                    detail: $"Profile with userId '{userId}' does not exist."
                );

            var result = mapper.Map<ProfileDto>(profile);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await GetProfile(userId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles")]
        public async Task<IActionResult> GetProfiles([FromQuery] PaginationRequest request)
        {
            var query = dbContext.Profiles.AsQueryable();

            // reflection sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = PaginationHelper.ApplySorting(query, request.SortBy, request.SortDirection);

            var totalCount = await query.CountAsync();
            var profiles = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var result = new PaginatedResponse<ProfileSummaryDto>
            {
                Items = mapper.Map<List<ProfileSummaryDto>>(profiles),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto profile)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            // if user without Admin role tries to update another user account - error 
            if(profile.UserId != userId && (!userRole?.ToLower().Contains("Admin".ToLower()) ?? false))
                 return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Not allowed to update another user account",
                    detail: $"User with userId '{userId}' adn '{userRole}' is not allowed to update another user account"
                );

            try
            {
                var existing = true;
                var userProfile = await dbContext.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

                // Profile part update or create if not exists
                if (userProfile == null)
                {
                    existing = false;
                    userProfile = new Data.Models.Profile();
                    userProfile.UserId = profile.UserId;
                }

                userProfile.FirstName = profile.FirstName;
                userProfile.LastName = profile.LastName;
                userProfile.AvatarUrl = profile.AvatarUrl;
                userProfile.Birthday = profile.Birthday;
                userProfile.UpdatedAt = DateTime.UtcNow;

                if (!existing)
                    dbContext.Profiles.Add(userProfile);

                // Profile settings part update or create if not exists
                existing = true;
                var profileSettings = await dbContext.ProfileSettings.FirstOrDefaultAsync(s => s.UserId == profile.UserId);
                if (profileSettings == null)
                {
                    existing = false;
                    profileSettings = new ProfileSettings();
                    profileSettings.UserId = profile.UserId;
                }

                //profileSettings.Language = profile.Settings.Language;
                //profileSettings.Timezone = profile.Settings.Timezone;
                profileSettings.NotificationsEnabled = profile.Settings.NotificationsEnabled;

                if (!existing)
                    dbContext.ProfileSettings.Add(profileSettings);

                await dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                logger.LogError("Error while adding profile and profile settings", ex);
                return Problem(
                   statusCode: StatusCodes.Status500InternalServerError,
                   title: "Error while adding profile and profile setting",
                   detail: $"Error while adding profile and profile setting.");
            }

            try
            {
                foreach (var contact in profile.Contacts)
                {
                    var dbContact = await dbContext.ProfileContacts.FirstOrDefaultAsync(c => c.Id == contact.Id);

                    if (dbContact != null && contact.ContactTypeId == (int)ContactTypeEnum.PrimaryEmail)
                        continue;

                    if (dbContact != null)
                        dbContact.Value = contact.Value;
                    else
                        dbContext.ProfileContacts.Add(new ProfileContact
                        {
                            UserId = profile.UserId,
                            ContactTypeId = contact.ContactTypeId,
                            Value = contact.Value
                        });

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error while adding contacts", ex);
                return Problem(
                   statusCode: StatusCodes.Status500InternalServerError,
                   title: "Error while adding contacts",
                   detail: $"Server error while adding contacts");
            }

            try
            {
                foreach (var address in profile.Addresses)
                {
                    var dbAddress = await dbContext.ProfileAddresses.FirstOrDefaultAsync(a => a.Id == address.Id);
                    if (dbAddress != null)
                    {
                        dbAddress.AddressLine = address.AddressLine;
                        dbAddress.City = address.City;
                        //dbAddress.Country = address.Country;
                        dbAddress.ZipCode = address.ZipCode;
                    }
                    else
                        dbContext.ProfileAddresses.Add(new ProfileAddress
                        {
                            UserId = profile.UserId,
                            AddressTypeId = address.AddressTypeId,
                            AddressLine = address.AddressLine,
                            City = address.City,
                            //Country = address.Country,
                            ZipCode = address.ZipCode
                        });

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error while adding addresses", ex);
                return Problem(
                   statusCode: StatusCodes.Status500InternalServerError,
                   title: "Error while adding addresses",
                   detail: $"Server error while adding addresses");
            }

            return Ok();
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:Guid}/contacts/{contactId:int}")]
        public async Task<IActionResult> GetContact(Guid userId, int contactId)
        {
            var contact = await dbContext.ProfileContacts.FirstOrDefaultAsync(c => c.Id == contactId);
            if(contact == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Contact not found",
                    detail: $"Contact with id '{contactId}' does not exist."
                );

            var result = mapper.Map<ProfileContactDto>(contact);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me/contacts/{contactId:int}")]
        public async Task<IActionResult> GetContact(int contactId)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await GetContact(userId, contactId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:Guid}/contacts")]
        public async Task<IActionResult> GetContacts(Guid userId)
        {
            var contacts = await dbContext.ProfileContacts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (contacts == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Contacts not found",
                    detail: $"Contact with userId '{userId}' does not exist."
                );

            var result = mapper.Map<List<ProfileContactDto>>(contacts);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me/contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await GetContacts(userId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles/{userId:Guid}/contacts")]
        public async Task<IActionResult> AddUpdateContact(Guid userId, [FromBody]ProfileContactDto contact)
        {
            var dbContact = await dbContext.ProfileContacts.FirstOrDefaultAsync(c => c.Id == contact.Id);
            if(dbContact == null)
            {
                dbContact = new ProfileContact();
                dbContact.UserId = userId;
                dbContact.ContactTypeId = contact.ContactTypeId;
            }

            dbContact.Value = contact.Value;
            await dbContext.SaveChangesAsync();

            var result = mapper.Map<ProfileContactDto>(dbContact);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles/me/contacts")]
        public async Task<IActionResult> AddUpdateContact([FromBody] ProfileContactDto contact)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await AddUpdateContact(userId, contact);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:Guid}/adresses/{addressId:int}")]
        public async Task<IActionResult> GetAddress(Guid userId, int addressId)
        {
            var adress = await dbContext.ProfileAddresses.FirstOrDefaultAsync(c => c.Id == addressId);
            if (adress == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Contact not found",
                    detail: $"Contact with id '{addressId}' does not exist."
                );

            var result = mapper.Map<ProfileAddressDto>(adress);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me/adresses/{addressId:int}")]
        public async Task<IActionResult> GetAddress(int addressId)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await GetAddress(userId, addressId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:Guid}/adresses")]
        public async Task<IActionResult> GetAddresses(Guid userId)
        {
            var contacts = await dbContext.ProfileAddresses.FirstOrDefaultAsync(c => c.UserId == userId);
            if (contacts == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Contacts not found",
                    detail: $"Contact with userId '{userId}' does not exist."
                );

            var result = mapper.Map<List<ProfileAddressDto>>(contacts);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me/adresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await GetAddresses(userId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles/{userId:Guid}/addresses")]
        public async Task<IActionResult> AddUpdateAddress(Guid userId, [FromBody] ProfileAddressDto address)
        {
            var dbAddress = await dbContext.ProfileAddresses.FirstOrDefaultAsync(c => c.Id == address.Id);
            if (dbAddress == null)
            {
                dbAddress = new ProfileAddress();
                dbAddress.UserId = userId;
                dbAddress.AddressTypeId = address.AddressTypeId;
            }

            dbAddress.AddressLine = address.AddressLine;
            //dbAddress.Country = address.Country;
            dbAddress.City = address.City;
            dbAddress.ZipCode = address.ZipCode;
            await dbContext.SaveChangesAsync();

            var result = mapper.Map<ProfileAddressDto>(dbAddress);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles/me/addresses")]
        public async Task<IActionResult> AddUpdateAddress([FromBody] ProfileContactDto contact)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await AddUpdateContact(userId, contact);
        }


        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await dbContext.Languages.ToListAsync();
            var result = mapper.Map<List<LanguageDto>>(languages);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/timezones")]
        public async Task<IActionResult> GetTimezones()
        {
            var timezones = await dbContext.Timezones.ToListAsync();
            var result = mapper.Map<List<TimezoneDto>>(timezones);

            return Ok(result);
        }

    }
}
