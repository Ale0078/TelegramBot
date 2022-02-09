using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class AnswerProfile : Profile
    {
        public AnswerProfile()
        {
            CreateMap<Bot.Models.Answer, Bot.Entities.Answer>();
            CreateMap<Bot.Entities.Answer, Bot.Models.Answer>();
        }
    }
}
