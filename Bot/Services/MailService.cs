using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

using Bot.Entities;
using Bot.Datas;

namespace Bot.Services
{
    public class MailService
    {
        private readonly Timer _timer;
        private readonly ITelegramBotClient _englishBotClient;
        private readonly ConcurrentDictionary<long, string> _futureMails;
        private readonly SynchronizedCollection<MailingData> _currentMails;
        private readonly ApplicationContext _context;

        private object _locker;

        public MailService(string englishBotToken, ApplicationContext context)
        {
            _englishBotClient = new TelegramBotClient(englishBotToken);
            _locker = new object();
            _futureMails = new ConcurrentDictionary<long, string>();
            _currentMails = new SynchronizedCollection<MailingData>();
            _context = context;
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

        public void FinishCreatingMailing(long chatId, DateTime dateOfMailing) 
        {
            _futureMails.TryRemove(chatId, out string message);

            _currentMails.Add(new MailingData
            {
                Message = message,
                DateOfMailing = dateOfMailing
            });
        }

        private void DoMailingMessageToUsers(object parametr) 
        {
            if (!_currentMails.Any(data => data.DateOfMailing <= DateTime.Now))
            {
                return;
            }

            IEnumerable<MailingData> mailingDatas = new List<MailingData>(_currentMails
                .Where(data => data.DateOfMailing <= DateTime.Now)
                .AsEnumerable());
            IEnumerable<long> chatIds = GetChatIds();

            foreach (MailingData data in mailingDatas)
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
        }

        private IEnumerable<long> GetChatIds() => _context.Chats
            .AsNoTracking()
            .Select<Chat, long>(chat => chat.Id)
            .AsEnumerable();
    }
}
