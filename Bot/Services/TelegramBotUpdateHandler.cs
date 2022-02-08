using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Services
{
    public class TelegramBotUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramBotUpdateHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            throw new NotImplementedException();
        }

        public async Task HandleUpdateAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(_botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
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
