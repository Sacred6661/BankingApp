using AutoMapper;
using ProfileService.Data.Models;
using ProfileService.DTOs;

namespace ProfileService.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Data.Models.Profile, ProfileDto>()
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<ProfileContact, ProfileContactDto>();

            CreateMap<ProfileAddress, ProfileAddressDto>()
                .ForMember(dest => dest.AddessTypeName,
                    opt => opt.MapFrom(src => src.AddressType.TypeName))
                .ForMember(dest => dest.AddressTypeId,
                    opt => opt.MapFrom(src => src.AddressType.Id));

            CreateMap<ProfileContact, ProfileContactDto>()
            .ForMember(dest => dest.ContactTypeName,
                opt => opt.MapFrom(src => src.ContactType.TypeName))
            .ForMember(dest => dest.ContactTypeId,
                opt => opt.MapFrom(src => src.ContactType.Id));

            CreateMap<ProfileSettings, ProfileSettingsDto>();
        }
    }
}

