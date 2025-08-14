using AutoMapper;
using TransactionService.Data.Models;
using TransactionService.DTOs;

namespace TransactionService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Transaction -> TransactionDto
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.TransactionType,
                    opt => opt.MapFrom(src => src.TransactionTypeEnum))
                .ForMember(dest => dest.TransactionStatus,
                    opt => opt.MapFrom(src => src.TransactionStatusEnum))
                .ReverseMap()
                .ForMember(dest => dest.TransactionTypeEnum,
                    opt => opt.MapFrom(src => src.TransactionType))
                .ForMember(dest => dest.TransactionStatusEnum,
                    opt => opt.MapFrom(src => src.TransactionStatus));
        }
    }
}
