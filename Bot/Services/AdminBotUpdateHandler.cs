using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

                await HandleErrorAsync(botClient, ex, cancellationToken);
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
                await OnContinueAddingUser(botClient, message);

                return;
            }

            if (_adminUserService.DoesUserStartSettingAdminUser(message.Chat.Id))
            {
                await OnContinueSettingAdminUser(botClient, message);

                return;
            }

            if (_adminUserService.DoesUserStartRemovingAdminRole(message.Chat.Id))
            {
                await OnContinueSoftRemoveUser(botClient, message);

                return;
            }

            await ChooseCommand(botClient, message, admin);
        }

        public Task ChooseCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            Task action;

            action = message.Text switch
            {
                AdminCommandList.START_COMMAND => OnStartCommand(botClient, message, admin),
                AdminCommandList.HALPE_COMMAND => OnHelpCommand(botClient, message, admin),
                _ => ChooseFromOwnerCommands(botClient, message, admin)
            };

            return action;
        }

        public Task ChooseFromOwnerCommands(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            Task action = null;

            if (admin.Role is UserRole.Owner or UserRole.Developer)
            {
                action = message.Text switch
                {
                    AdminCommandList.ADD_USER => OnAddUser(botClient, message),
                    AdminCommandList.SET_ADMIN_USER => OnSetAdminUser(botClient, message),
                    AdminCommandList.SOFT_REMOVE_ADMIN_USER => OnSoftRemoveUser(botClient, message)
                };
            }

            return action;
        }

        public async Task OnAddUser(ITelegramBotClient botClient, Message message)
        {
            _adminUserService.StartAddingUser(message.Chat.Id);

            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["AddingUser"]);
        }

        public async Task OnContinueAddingUser(ITelegramBotClient botClient, Message message)
        {
            string text;

            if (_adminUserService.DoesUserExist(message.Text))
            {
                text = string.Format(_resourceReader["ErrorWithAddingUser"], message.Text);

                _adminUserService.BreakAddingUser(message.Chat.Id);
            }
            else 
            {
                text = string.Format(_resourceReader["FinishAddingUser"], message.Text);

                await _adminUserService.AddUser(message.Text, message.Chat.Id);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, text);
        }

        public async Task OnSetAdminUser(ITelegramBotClient botClient, Message message) 
        {
            if (!_adminUserService.HaveAnyNoAdminUsers())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["DontHaveAnyNoAdminUser"]);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["SettingAdminUser"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsToAdminUsers(
                    admins: _adminUserService.GetAdminByRole(UserRole.Default),
                    columns: 2)));

            _adminUserService.StartSettingAdminUser(message.Chat.Id);
        }

        public async Task OnContinueSettingAdminUser(ITelegramBotClient botClient, Message message) 
        {
            string text;

            if (_adminUserService.DoesUserExist(message.Text))
            {
                text = string.Format(_resourceReader["FinishSettingAdminUser"], message.Text);

                await _adminUserService.SetRole(message.Text, UserRole.Admin);
            }
            else 
            {
                text = string.Format(_resourceReader["ErrorSettingAdminUser"], message.Text);
            }

            await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: new ReplyKeyboardRemove());

            _adminUserService.BreakSettingAdminUser(message.Chat.Id);

            return;
        }

        public async Task OnSoftRemoveUser(ITelegramBotClient botClient, Message message) 
        {
            if (!_adminUserService.HaveAnyAdminUsers())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["DontHaveAnyAdminUsers"]);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["SoftRemoveAdmin"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsToAdminUsers(
                    admins: _adminUserService.GetAdminByRole(UserRole.Admin),
                    columns: 2)));

            _adminUserService.StartRemovingAdminRole(message.Chat.Id);
        }

        public async Task OnContinueSoftRemoveUser(ITelegramBotClient botClient, Message message) 
        {
            string text;

            if (_adminUserService.DoesUserExist(message.Text))
            {
                text = string.Format(_resourceReader["FinishSodtRemoveAdmin"], message.Text);

                await _adminUserService.SetRole(message.Text, UserRole.Default);
            }
            else 
            {
                text = string.Format(_resourceReader["ErrorSoftRemoveAdmin"], message.Text);
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());

            _adminUserService.BreakRemovingAdminRole(message.Chat.Id);
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
            await botClient.SendTextMessageAsync(message.Chat.Id, GetHelpMessage(admin.Role));
        }

        #region ============================= Helper Functions ====================================================

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

                builder.Append(AdminCommandList.REMOVE_USER);
                builder.Append(" - ");
                builder.Append(_resourceReader["RemoveUserDescription"]);

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

        private KeyboardButton[][] GetKeyboardsToAdminUsers(IList<AdminUser> admins, int columns) 
        {
            KeyboardButton[][] keyboards = new KeyboardButton[admins.Count][];

            for (int i = 0; i < admins.Count; i++)
            {
                KeyboardButton[] buttons;

                if (admins.Skip(i * columns).Count() >= columns)
                {
                    buttons = new KeyboardButton[columns];
                }
                else
                {
                    buttons = new KeyboardButton[admins.Skip(i * columns).Count()];
                }

                for (int j = 0; j < columns && i * columns + j < admins.Count; j++)
                {
                    StringBuilder stringBuilder = new('@');

                    stringBuilder.Append(admins[i * columns + j].UserName);

                    buttons[j] = stringBuilder.ToString();
                }

                keyboards[i] = buttons;
            }

            return keyboards;
        }

        #endregion
    }
}
