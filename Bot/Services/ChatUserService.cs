using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Data;
using Bot.Entities;

namespace Bot.Services
{
    public class ChatUserService
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;

        public ChatUserService(ApplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Models.ChatUser>> GetUsersByCommingResource(UserComingResource? comingResource)
        {
            IEnumerable<ChatUser> users;

            if (comingResource is null)
            {
                users = await _context.ChatUsers
                    .Include(x => x.Chat)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                users = _context.ChatUsers
                    .Include(x => x.Chat)
                    .AsNoTracking()
                    .Where(user => user.From == comingResource);
            }

            return _mapper.Map<IEnumerable<Models.ChatUser>>(users);
        }

        public async Task<bool> DoesUserFromCommitgResourceExist(UserComingResource? comingResource)
        {
            bool doesExist = false;

            if (comingResource is null)
            {
                doesExist = await _context.ChatUsers
                    .AsNoTracking()
                    .CountAsync() > 0;
            }
            else
            {
                doesExist = await _context.ChatUsers
                    .AsNoTracking()
                    .Where(user => user.From == comingResource)
                    .CountAsync() > 0;
            }

            return doesExist;
        }
    }
}
