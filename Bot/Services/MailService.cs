using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AutoMapper;
using Telegram.Bot;
using Z.EntityFramework.Plus;

using Bot.Models;
using Bot.Extentions;

namespace Bot.Services
{
    public class MailService
    {
        private readonly Timer _timer;
        private readonly ITelegramBotClient _englishBotClient;
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<long, string> _futureMails;
        private readonly SynchronizedCollection<Mailing> _currentMails;
        private readonly Entities.ApplicationContext _context;

        private object _locker;

        public MailService(string englishBotToken, Entities.ApplicationContext context, IMapper mapper)
        {
            _englishBotClient = new TelegramBotClient(englishBotToken);
            _locker = new object();
            _futureMails = new ConcurrentDictionary<long, string>();
            _currentMails = new SynchronizedCollection<Mailing>();
            _context = context;
            _mapper = mapper;

            LoadMailings();

            _timer = new Timer(
                callback: DoMailingMessageToUsers,
                state: null,
                dueTime: TimeSpan.Zero,
                period: new TimeSpan(0, 1, 0));
        }

        public Task DoMailingImmediately(string message) 
        {
            IEnumerable<long> chatIds = GetChatIds();

            lock (_locker)
            {
                foreach (long chatId in chatIds)
                {
                    _englishBotClient.SendTextMessageAsync(chatId, message);
                }
            }

            return Task.CompletedTask;
        }

        public void StartCreatingMailing(long chatId, string message) 
        {
            _futureMails.TryAdd(chatId, message);
        }

        public async Task FinishCreatingMailing(long chatId, DateTime dateOfMailing) 
        {
            _futureMails.TryRemove(chatId, out string message);

            EntityEntry<Entities.Mailing> entity = _context.Mailings.Add(new Entities.Mailing
            {
                Message = message,
                DateOfMailing = dateOfMailing
            });

            await _context.SaveChangesAsync();

            _currentMails.Add(_mapper.Map<Mailing>(entity.Entity));
        }

        private void DoMailingMessageToUsers(object parametr) 
        {
            if (!_currentMails.Any(data => data.DateOfMailing <= DateTime.UtcNow.GetUkrainianTimeFromUtc()))
            {
                return;
            }

            DateTime timeOfMailing = DateTime.UtcNow.GetUkrainianTimeFromUtc();

            IEnumerable<Mailing> mailingDatas = new List<Mailing>(_currentMails
                .Where(data => data.DateOfMailing <= timeOfMailing)
                .AsEnumerable());
            IEnumerable<long> chatIds = GetChatIds();

            foreach (Mailing data in mailingDatas)
            {
                lock (_locker)
                {
                    foreach (long chatId in chatIds)
                    {
                        _englishBotClient.SendTextMessageAsync(chatId, data.Message);
                    }
                }

                _currentMails.Remove(data);
            }

            _context.Mailings
                .Where(mailing => mailing.DateOfMailing <= timeOfMailing)
                .Delete();
        }

        private IEnumerable<long> GetChatIds() => _context.Chats
            .AsNoTracking()
            .Select<Entities.Chat, long>(chat => chat.Id)
            .AsEnumerable();

        private void LoadMailings() 
        {
            IEnumerable<Mailing> mailings = _mapper.Map<List<Mailing>>(_context.Mailings
                .AsNoTracking()
                .ToList());

            foreach (Mailing mailing in mailings) 
            {
                _currentMails.Add(mailing);
            }
        }
    }
}
