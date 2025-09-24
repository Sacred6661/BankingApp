using Mapster;
using ProfileService.Data.Models;
using ProfileService.DTOs;

namespace ProfileService.Mapping
{
    public class MappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Profile -> ProfileDto
            config.ForType<Profile, ProfileDto>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            config.ForType<ProfileDto, Profile>();

            // ProfileContact -> ProfileContactDto
            config.ForType<ProfileContact, ProfileContactDto>()
                .Map(dest => dest.ContactTypeName, src => src.ContactType.TypeName)
                .Map(dest => dest.ContactTypeId, src => src.ContactType.Id);

            // ProfileAddress -> ProfileAddressDto
            config.ForType<ProfileAddress, ProfileAddressDto>()
                .Map(dest => dest.AddessTypeName, src => src.AddressType.TypeName)
                .Map(dest => dest.AddressTypeId, src => src.AddressType.Id)
                .Map(dest => dest.CountryName, src => src.Country.Name)
                .Map(dest => dest.AddressTypeId, src => src.AddressType.Id); ;

            // ProfileSettings -> ProfileSettingsDto (без кастомних мап)
            config.ForType<ProfileSettings, ProfileSettingsDto>()
                .Map(dest => dest.LanguageName, src => src.Language.Name)
                .Map(dest => dest.LanguageId, src => src.Language.Id)
                .Map(dest => dest.TimezoneName, src => src.Timezone.Name)
                .Map(dest => dest.TimezoneId, src => src.Timezone.Id);

            config.ForType<Language, LanguageDto>();
            config.ForType<Timezone, TimezoneDto>();
            config.ForType<Country, CountryDto>();
            config.ForType<AddressType, AddressTypeDto>();
            config.ForType<ContactType, ContactTypeDto>();
        }
    }
}

