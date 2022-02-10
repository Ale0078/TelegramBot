using Bot.Entities;

namespace Bot.Services
{
    public class ChatService
    {
        private readonly ApplicationContext _context;

        public ChatService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task AddChat(long id) 
        {
            await _context.Chats.AddAsync(new Chat { Id = id });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveChat(long id) 
        {
            _context.Chats.Remove(_context.Chats.Find(id));

            await _context.SaveChangesAsync();
        }

        public bool DoesExist(long id) 
        {
            return _context.Chats.Any(chat => chat.Id == id);
        }

        public async Task MailAllUnmaledChats() 
        {
            IQueryable<Chat> chats = _context.Chats.Where(chat => !chat.DoesGetMail);

            foreach (Chat chat in chats) 
            {
                chat.DoesGetMail = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task FinishTest(long id) 
        {
            Chat chat = await _context.Chats.FindAsync(id);

            chat.DoesFinishTest = true;

            await _context.SaveChangesAsync();
        }
    }
}
