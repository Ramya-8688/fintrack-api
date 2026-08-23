using AutoMapper;
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for application DTOs and entities.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Configures DTO mappings.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Transaction, TransactionDto>().ReverseMap();
            CreateMap<SharedExpense, ExpenseDto>().ReverseMap();
            CreateMap<ExpenseParticipant, ParticipantDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
