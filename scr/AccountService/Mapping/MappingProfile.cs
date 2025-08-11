using AccountService.Data.Models;
using AccountService.DTOs;
using AutoMapper;

namespace AccountService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Account, AccountsDto>();
            CreateMap<AccountsDto, Account>();
        }
    }
}
