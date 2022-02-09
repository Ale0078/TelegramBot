using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

using Bot.Datas;
using Bot.Entities;

namespace Bot.Services
{
    public class BotUpdateHandler : IUpdateHandler
    {
        private readonly ConcurrentDictionary<long, MarkScore> _participants;
        private readonly ApplicationContext _context;
        private readonly ResourceReader _resourceReader;

        public BotUpdateHandler(ApplicationContext context, ResourceReader resourceReader)
        {
            _context = context;
            _resourceReader = resourceReader;

            _participants = new ConcurrentDictionary<long, MarkScore>();
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception ex) 
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        private async Task OnMessageReceived(ITelegramBotClient botClient, Update update) 
        {
            if (update.Message.Type is not MessageType.Text)
            {
                return;
            }

            var action = update.Message.Text switch
            {
                "/start" => Start(botClient, update.Message)
            };

            await action;
        }

        private async Task Start(ITelegramBotClient botClient, Message message)
        {
            if (_participants.ContainsKey(message.Chat.Id))
            {
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["TestIsStarted"]);

            _participants.TryAdd(message.Chat.Id, new MarkScore());

            ReplyKeyboardMarkup markup = new(new[]
            {
                new KeyboardButton[] { "He came" },
                new KeyboardButton[] { "He already came" },
                new KeyboardButton[] { "He has come" }
            });

            await botClient.SendTextMessageAsync(message.Chat.Id, "Hi", replyMarkup: markup);
        }
    }
}
