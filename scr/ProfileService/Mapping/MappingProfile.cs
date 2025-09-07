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

            // ProfileContact -> ProfileContactDto
            config.ForType<ProfileContact, ProfileContactDto>()
                .Map(dest => dest.ContactTypeName, src => src.ContactType.TypeName)
                .Map(dest => dest.ContactTypeId, src => src.ContactType.Id);

            // ProfileAddress -> ProfileAddressDto
            config.ForType<ProfileAddress, ProfileAddressDto>()
                .Map(dest => dest.AddessTypeName, src => src.AddressType.TypeName)
                .Map(dest => dest.AddressTypeId, src => src.AddressType.Id)
                .Map(dest => dest.CountryName, src => src.C.TypeName)
                .Map(dest => dest.AddressTypeId, src => src.AddressType.Id); ;

            // ProfileSettings -> ProfileSettingsDto (без кастомних мап)
            config.ForType<ProfileSettings, ProfileSettingsDto>();

            config.ForType<Language, LanguageDto>();
            config.ForType<Timezone, TimezoneDto>();
            config.ForType<Country, CountryDto>();
        }
    }
}

