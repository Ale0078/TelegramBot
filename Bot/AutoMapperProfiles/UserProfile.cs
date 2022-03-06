using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Bot.Entities.ChatUser, Bot.Models.ChatUser>();
            CreateMap<Bot.Models.ChatUser, Bot.Entities.ChatUser>();

            CreateMap<Bot.Entities.AdminUser, Bot.Models.AdminUser>();
            CreateMap<Bot.Models.AdminUser, Bot.Entities.AdminUser>();
        }
    }
}
