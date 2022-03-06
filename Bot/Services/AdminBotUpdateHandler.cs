using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Bot.Datas;

namespace Bot.Services
{
    public class AdminBotUpdateHandler : IUpdateHandler
    {
        private const string ADMIN_ID = "ADMIN_ID";

        private readonly ResourceReader _resourceReader;

        public AdminBotUpdateHandler(ResourceReader resourceReader)
        {
            _resourceReader = resourceReader;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: (long)exception.Data[ADMIN_ID],
                text: _resourceReader["ExceptionMessage"]);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => OnMessageRecive(botClient, update.Message)
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                ex.Data.Add(ADMIN_ID, update.Message.Chat.Id);

                HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        public async Task OnMessageRecive(ITelegramBotClient botClient, Message message) 
        {
            if (message.Type is not MessageType.Text)
            {
                return;
            }

            await ChooseCommand(botClient, message);
        }

        public Task ChooseCommand(ITelegramBotClient botClient, Message message) 
        {
            Task action = message.Text switch
            {
                AdminCommandList.START_COMMAND => OnStartCommand(botClient, message.Chat.Id)
            };

            return action;
        }

        public async Task OnStartCommand(ITelegramBotClient botClient, long chatId) 
        {
            await botClient.SendTextMessageAsync(chatId, GetHelpMessage());
        }

        private string GetHelpMessage() 
        {
            StringBuilder builder = new();

            builder.Append(AdminCommandList.GET_USERS_FROM_INSTAGRAM_COMMAND);
            builder.Append(" - ");
            builder.Append(_resourceReader["GetUserFromInstagramDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.GET_USERS_FROM_INSTAGRAM_COMMAND_AS_FILE);
            builder.Append(" - ");
            builder.Append(_resourceReader["GetUserFromInstagramAsFileDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.HALPE_COMMAND);
            builder.Append(" - ");
            builder.Append(_resourceReader["HelpDescription"]);

            return builder.ToString();
        }
    }
}
