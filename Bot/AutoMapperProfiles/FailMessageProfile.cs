using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class FailMessageProfile : Profile
    {
        public FailMessageProfile()
        {
            CreateMap<Bot.Models.FailMessage, Bot.Entities.FailMessage>();
            CreateMap<Bot.Entities.FailMessage, Bot.Models.FailMessage>();
        }
    }
}
