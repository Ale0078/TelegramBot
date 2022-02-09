using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Bot.Models.Question, Bot.Entities.Question>();
            CreateMap<Bot.Entities.Question, Bot.Models.Question>();
        }
    }
}
