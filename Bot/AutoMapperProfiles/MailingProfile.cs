using AutoMapper;

namespace Bot.AutoMapperProfiles
{
    public class MailingProfile : Profile
    {
        public MailingProfile()
        {
            CreateMap<Bot.Models.Mailing, Bot.Entities.Mailing>();
            CreateMap<Bot.Entities.Mailing, Bot.Models.Mailing>();
        }
    }
}
