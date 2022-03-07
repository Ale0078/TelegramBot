using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Bot.Datas;
using Bot.Data;
using Bot.Models;

namespace Bot.Services
{
    public class AdminBotUpdateHandler : IUpdateHandler
    {
        private const string ADMIN_ID = "ADMIN_ID";

        private readonly ResourceReader _resourceReader;
        private readonly AdminUserService _adminUserService;

        public AdminBotUpdateHandler(ResourceReader resourceReader, AdminUserService adminUserService)
        {
            _resourceReader = resourceReader;
            _adminUserService = adminUserService;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: (long)exception.Data[ADMIN_ID],
                text: _resourceReader["ExceptionMessage"]);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            AdminUser admin = await DoesUserExist(botClient, update.Message);

            if (admin is null || !_adminUserService.IsUserAdmin(admin))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, _resourceReader["ExceptionMessage"]);

                return;
            }

            await UpdateUser(admin, update.Message.From.Username);

            //Task handler = update.Type switch
            //{
            //    UpdateType.Message => OnMessageRecive(botClient, update.Message)
            //};
            Task handler = OnMessageRecive(botClient, update.Message, admin);

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

        public async Task OnMessageRecive(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            if (message.Type is not MessageType.Text)
            {
                return;
            }

            if (_adminUserService.DoesUserStartAddingUser(message.Chat.Id))
            {
                await OnContinueAddingUser(botClient, message, admin);

                return;
            }

            await ChooseCommand(botClient, message, admin);
        }

        public Task ChooseCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            Task action;

            if (admin.Role is UserRole.Owner or UserRole.Developer)
            {
                action = message.Text switch
                {
                    AdminCommandList.ADD_USER => OnAddUser(botClient, message, admin)
                };

                if (admin is not null)
                {
                    return action;
                }
            }

            action = message.Text switch
            {
                AdminCommandList.START_COMMAND => OnStartCommand(botClient, message, admin),
                AdminCommandList.HALPE_COMMAND => OnHelpCommand(botClient, message, admin)
            };

            return action;
        }

        public async Task OnStartCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            if (_adminUserService.DoesUserNeedFinishRegistration(admin))
            {
                await _adminUserService.FinishUserRegistration(admin, message.Chat.Id);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, GetHelpMessage(admin.Role));
        }

        public async Task OnHelpCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            await UpdateUser(admin, message.From.Username);

            await botClient.SendTextMessageAsync(message.Chat.Id, GetHelpMessage(admin.Role));
        }

        public async Task OnAddUser(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            await UpdateUser(admin, message.From.Username);

            _adminUserService.StartAddingUser(message.Chat.Id);

            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["AddingUser"]);
        }

        public async Task OnContinueAddingUser(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            await UpdateUser(admin, message.From.Username);

            if (_adminUserService.DoesUserExist(message.Text))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: String.Format(_resourceReader["ErrorWithAddingUser"], message.Text));

                _adminUserService.BreakAddingUser(message.Chat.Id);

                return;
            }

            await _adminUserService.AddUser(message.Text, message.Chat.Id);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: String.Format(_resourceReader["FinishAddingUser"], message.Text));
        }

        private string GetHelpMessage(UserRole role) 
        {
            StringBuilder builder = new();

            if (role is UserRole.Owner or UserRole.Developer) 
            {
                builder.Append(AdminCommandList.ADD_USER);
                builder.Append(" - ");
                builder.Append(_resourceReader["AddUserDescription"]);

                builder.Append('\n');
                builder.Append('\n');

                builder.Append(AdminCommandList.SET_ADMIN_USER);
                builder.Append(" - ");
                builder.Append(_resourceReader["SetAdminUserDescription"]);

                builder.Append('\n');
                builder.Append('\n');

                builder.Append(AdminCommandList.SOFT_REMOVE_ADMIN_USER);
                builder.Append(" - ");
                builder.Append(_resourceReader["SoftRemoveAdminUserDescription"]);

                builder.Append('\n');
                builder.Append('\n');

                builder.Append(AdminCommandList.REMOVE_ADMIN_UESR);
                builder.Append(" - ");
                builder.Append(_resourceReader["RemoveAdminUserDescription"]);

                builder.Append('\n');
                builder.Append('\n');
            }

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

        private async Task<AdminUser> DoesUserExist(ITelegramBotClient botClient, Message message) 
        {
            if (!_adminUserService.DoesUserExist(message.Chat.Id))
            {
                if (!_adminUserService.DoesUserExist(message.From.Username))
                {
                    return null;
                }

                return await _adminUserService.GetAdminUser(message.From.Username);
            }
            else 
            {
                return await _adminUserService.GetAdminUser(message.Chat.Id);
            }
        }

        private async Task UpdateUser(AdminUser admin, string userName) 
        {
            if (!_adminUserService.DoesUserNeedUpdate(userName, admin))
            {
                return;
            }

            await _adminUserService.UpdateUser(userName, admin);
        }
    }
}
