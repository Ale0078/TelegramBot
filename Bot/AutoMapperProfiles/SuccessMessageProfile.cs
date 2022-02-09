using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class SuccessMessageProfile : Profile
    {
        public SuccessMessageProfile()
        {
            CreateMap<Bot.Entities.SuccessMessage, Bot.Models.SuccessMessage>();
            CreateMap<Bot.Models.SuccessMessage, Bot.Entities.SuccessMessage>();
        }
    }
}
