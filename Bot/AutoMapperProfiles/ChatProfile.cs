using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            CreateMap<Bot.Entities.Chat, Bot.Models.Chat>();
            CreateMap<Bot.Models.Chat, Bot.Entities.Chat>();
        }
    }
}
