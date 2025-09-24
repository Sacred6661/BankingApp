using MapsterMapper;
using MassTransit;
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
                .Include(p => p.Settings).ThenInclude(p => p.Language)
                .Include(p => p.Settings).ThenInclude(p => p.Timezone)
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

            var userProfile = await dbContext.Profiles.Include(p => p.Settings).FirstOrDefaultAsync(p => p.UserId == userId);

            if (userProfile == null)
            {
                var newProfile = new Data.Models.Profile
                {
                    UserId = userId,
                    Email = profile.Email,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    AvatarUrl = profile.AvatarUrl,
                    Birthday = profile.Birthday,
                    Contacts = new List<ProfileContact>(),
                    Settings = new ProfileSettings()
                };

                newProfile.Contacts.Add(new ProfileContact
                {
                    UserId = userId,
                    ContactTypeEnum = ContactTypeEnum.PrimaryEmail,
                    Value = profile.Email
                });

                newProfile.Settings = new ProfileSettings
                {
                    UserId = userId,
                    LanguageId = profile.Settings.LanguageId,
                    TimeZoneId = profile.Settings.TimezoneId,
                    NotificationsEnabled = profile.Settings.NotificationsEnabled
                };

                dbContext.Profiles.Add(newProfile);
                await dbContext.SaveChangesAsync();

                return Ok();
            }

            userProfile.FirstName = profile.FirstName;
            userProfile.LastName = profile.LastName;
            userProfile.AvatarUrl = profile.AvatarUrl;
            userProfile.Birthday = profile.Birthday;
            userProfile.UpdatedAt = DateTime.UtcNow;

            userProfile.Settings.TimeZoneId = profile.Settings.TimezoneId;
            userProfile.Settings.LanguageId = profile.Settings.LanguageId;
            userProfile.Settings.NotificationsEnabled = profile.Settings.NotificationsEnabled;

            await dbContext.SaveChangesAsync();

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
            var contacts = await dbContext.ProfileContacts.Include(c => c.ContactType).Where(c => c.UserId == userId)?.ToListAsync();
            if (contacts == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Contacts not found",
                    detail: $"Contacts with userId '{userId}' does not exist."
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
            var dbContact = await dbContext.ProfileContacts.Include(c => c.ContactType).FirstOrDefaultAsync(c => c.Id == contact.Id);
            if(dbContact == null)
            {
                dbContact = new ProfileContact();
                dbContact.UserId = userId;
                dbContact.ContactTypeId = contact.ContactTypeId;
                dbContact.Value = contact.Value;

                dbContext.Add(dbContact);
            }
            else
            {
                dbContact.Value = contact.Value;
            }


            await dbContext.SaveChangesAsync();

            await dbContext.Entry(dbContact).Reference(c => c.ContactType).LoadAsync();

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
        [HttpDelete("profiles/{userId:Guid}/contacts/{contactId:int}")]
        public async Task<IActionResult> DeleteContact(Guid userId, int contactId)
        {
            var dbContact = await dbContext.ProfileContacts.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == contactId);

            if(dbContact == null)
                return Problem(
                  statusCode: StatusCodes.Status404NotFound,
                  title: "Contact not found",
                  detail: $"Contact with id '{contactId}' does not exist."
              );

            dbContact.IsActive = false;
            await dbContext.SaveChangesAsync();

            var result = mapper.Map<ProfileContactDto>(dbContact);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpDelete("profiles/me/contacts/{contactId:int}")]
        public async Task<IActionResult> DeleteContact(int contactId)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await DeleteContact(userId, contactId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/{userId:Guid}/adresses/{addressId:int}")]
        public async Task<IActionResult> GetAddress(Guid userId, int addressId)
        {
            var adress = await dbContext.ProfileAddresses.FirstOrDefaultAsync(c => c.Id == addressId);
            if (adress == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Address not found",
                    detail: $"Address with id '{addressId}' does not exist."
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
        [HttpGet("profiles/{userId:Guid}/addresses")]
        public async Task<IActionResult> GetAddresses(Guid userId)
        {
            var contacts = await dbContext.ProfileAddresses.Include(c => c.AddressType)
                .Where(c => c.UserId == userId).ToListAsync();

            if (contacts == null)
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Address not found",
                    detail: $"Address with userId '{userId}' does not exist."
                );

            var result = mapper.Map<List<ProfileAddressDto>>(contacts);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/me/addresses")]
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

                dbAddress.AddressLine = address.AddressLine;
                dbAddress.CountryId = address.CountryId;
                dbAddress.City = address.City;
                dbAddress.ZipCode = address.ZipCode;

                dbContext.Add(dbAddress);
            }
            else
            {
                dbAddress.AddressLine = address.AddressLine;
                dbAddress.CountryId = address.CountryId;
                dbAddress.City = address.City;
                dbAddress.ZipCode = address.ZipCode;
            }

            await dbContext.SaveChangesAsync();

            await dbContext.Entry(dbAddress).Reference(c => c.AddressType).LoadAsync();
            await dbContext.Entry(dbAddress).Reference(c => c.Country).LoadAsync();

            var result = mapper.Map<ProfileAddressDto>(dbAddress);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpPut("profiles/me/addresses")]
        public async Task<IActionResult> AddUpdateAddress([FromBody] ProfileAddressDto contact)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await AddUpdateAddress(userId, contact);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpDelete("profiles/{userId:Guid}/addresses/{contactId:int}")]
        public async Task<IActionResult> DeleteAddress(Guid userId, int contactId)
        {
            var dbAddress = await dbContext.ProfileAddresses.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == contactId);

            if (dbAddress == null)
                return Problem(
                  statusCode: StatusCodes.Status404NotFound,
                  title: "Address not found",
                  detail: $"Address with id '{contactId}' does not exist."
              );

            dbAddress.IsActive = false;
            await dbContext.SaveChangesAsync();

            var result = mapper.Map<ProfileAddressDto>(dbAddress);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpDelete("profiles/me/addresses/{contactId:int}")]
        public async Task<IActionResult> DeleteAddress(int contactId)
        {
            var userId = Guid.Parse(User.FindFirst("user_id")?.Value);
            return await DeleteAddress(userId, contactId);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/addresstypes")]
        public async Task<IActionResult> GetAddressTypes()
        {
            var addressTypes = await dbContext.AddressTypes.Where(a => a.IsActive).ToListAsync();
            var result = mapper.Map<List<AddressTypeDto>>(addressTypes);

            return Ok(result);
        }

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/contacttypes")]
        public async Task<IActionResult> GetContactTypes()
        {
            var contactTypes = await dbContext.ContactTypes.Where(a => a.IsActive).ToListAsync();
            var result = mapper.Map<List<ContactTypeDto>>(contactTypes);

            return Ok(result);
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

        [Authorize(Policy = "RequireUserId")]
        [HttpGet("profiles/countries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await dbContext.Countries.ToListAsync();
            var result = mapper.Map<List<CountryDto>>(countries);

            return Ok(result);
        }

    }
}
