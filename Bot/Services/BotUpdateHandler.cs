using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;

namespace Bot.Services
{
    public class BotUpdateHandler : IUpdateHandler
    {
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
            var action = update.Message.Text switch
            {
                "/start" => Start(botClient, update.Message)
            };

            await action;

            async Task Start(ITelegramBotClient botClient, Message message) 
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Hello");
            }
        }
    }
}
