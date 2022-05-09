using System.Text;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;

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
        private readonly ChatUserService _chatUserService;
        private readonly MailService _mailService;

        public AdminBotUpdateHandler(ResourceReader resourceReader, AdminUserService adminUserService, 
            ChatUserService chatUserService, MailService mailService)
        {
            _resourceReader = resourceReader;
            _adminUserService = adminUserService;
            _chatUserService = chatUserService;
            _mailService = mailService;
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

        private async Task OnMessageRecive(ITelegramBotClient botClient, Message message, AdminUser admin)
        {
            if (message.Type is not MessageType.Text)
            {
                return;
            }

            Task handler;

            if (_adminUserService.TryGetExecutingCommand(message.Chat.Id, out ExecutingCommand command))
            {
                handler = ChooseContinueCommand(botClient, message, admin, command);
            }
            else
            {
                handler = ChooseCommand(botClient, message, admin);
            }

            await handler;
        }

        private Task ChooseContinueCommand(ITelegramBotClient botClient, Message message, AdminUser admin, ExecutingCommand command)
        {
            Task action;

            action = command switch
            {
                ExecutingCommand.GettingUsers => OnContinueGettingUsers(botClient, message),
                ExecutingCommand.GettingUsersAsFile => OnContinueGettingUsersAsFile(botClient, message),
                ExecutingCommand.DoingMailingImmediatly => OnContinueDoingMailingImmediatly(botClient, message),
                _ => ChooseFromOwnerContinueCommands(botClient, message, admin, command)
            };

            return action;
        }

        private Task ChooseFromOwnerContinueCommands(ITelegramBotClient botClient, Message message, AdminUser admin, ExecutingCommand command)
        {
            Task action = null;

            if (admin.Role is UserRole.Owner or UserRole.Developer)
            {
                action = command switch
                {
                    ExecutingCommand.Adding => OnContinueAddingUser(botClient, message),
                    ExecutingCommand.SettingAdminRole => OnContinueSettingAdminUser(botClient, message),
                    ExecutingCommand.SoftRemoning => OnContinueSoftRemoveUser(botClient, message),
                    ExecutingCommand.Removing => OnContinueRemovingUser(botClient, message)
                };
            }

            return action;
        }

        private Task ChooseCommand(ITelegramBotClient botClient, Message message, AdminUser admin)
        {
            Task action;

            action = message.Text switch
            {
                AdminCommandList.START_COMMAND => OnStartCommand(botClient, message, admin),
                AdminCommandList.HALPE_COMMAND => OnHelpCommand(botClient, message, admin),
                AdminCommandList.GET_USERS_COMMAND => OnGetUsers(botClient, message),
                AdminCommandList.GET_USERS_COMMAND_AS_FILE => OnGetUsersAsFile(botClient, message),
                AdminCommandList.DO_MAILING_IMMEDIATLY => OnDoMailingImmediatly(botClient, message),
                _ => ChooseFromOwnerCommands(botClient, message, admin)
            };

            return action;
        }

        private Task ChooseFromOwnerCommands(ITelegramBotClient botClient, Message message, AdminUser admin)
        {
            Task action = null;

            if (admin.Role is UserRole.Owner or UserRole.Developer)
            {
                action = message.Text switch
                {
                    AdminCommandList.ADD_USER => OnAddUser(botClient, message),
                    AdminCommandList.SET_ADMIN_USER => OnSetAdminUser(botClient, message),
                    AdminCommandList.SOFT_REMOVE_ADMIN_USER => OnSoftRemoveUser(botClient, message),
                    AdminCommandList.REMOVE_USER => OnRemoveUser(botClient, message)
                };
            }

            return action;
        }

        private async Task OnAddUser(ITelegramBotClient botClient, Message message)
        {
            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.Adding);

            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["AddingUser"]);
        }

        private async Task OnContinueAddingUser(ITelegramBotClient botClient, Message message)
        {
            string text;

            if (_adminUserService.DoesUserExist(message.Text))
            {
                text = string.Format(_resourceReader["ErrorWithAddingUser"], message.Text);

                _adminUserService.StopExecutingCommand(message.Chat.Id);
            }
            else
            {
                text = string.Format(_resourceReader["FinishAddingUser"], message.Text);

                await _adminUserService.AddUser(message.Text, message.Chat.Id);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, text);
        }

        private async Task OnSetAdminUser(ITelegramBotClient botClient, Message message)
        {
            if (!_adminUserService.HaveAnyNoAdminUsers())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["DontHaveAnyNoAdminUser"]);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["SettingAdminUser"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsFromAdminUsers(
                    admins: _adminUserService.GetAdminByRole(UserRole.Default),
                    columns: 2)));

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.SettingAdminRole);
        }

        private async Task OnContinueSettingAdminUser(ITelegramBotClient botClient, Message message)
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

            _adminUserService.StopExecutingCommand(message.Chat.Id);

            return;
        }

        private async Task OnSoftRemoveUser(ITelegramBotClient botClient, Message message)
        {
            if (!_adminUserService.HaveAnyAdminUsers())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["DontHaveAnyAdminUsers"]);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["SoftRemoveAdmin"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsFromAdminUsers(
                    admins: _adminUserService.GetAdminByRole(UserRole.Admin),
                    columns: 2)));

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.SoftRemoning);
        }

        private async Task OnContinueSoftRemoveUser(ITelegramBotClient botClient, Message message)
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

            _adminUserService.StopExecutingCommand(message.Chat.Id);
        }

        private async Task OnRemoveUser(ITelegramBotClient botClient, Message message)
        {
            if (!_adminUserService.HaveAnyNoAdminUsers())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["DontHaveUsersToRemove"]);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["RemoveUser"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsFromAdminUsers(
                    admins: _adminUserService.GetAdminByRole(UserRole.Default),
                    columns: 2)));

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.Removing);
        }

        private async Task OnContinueRemovingUser(ITelegramBotClient botClient, Message message)
        {
            string text;

            if (_adminUserService.DoesUserExist(message.Text))
            {
                AdminUser user = await _adminUserService.RemoveUser(message.Text);

                text = string.Format(_resourceReader["FinishRemovingUser"], user.UserName);
            }
            else
            {
                text = string.Format(_resourceReader["ErrorRemovingUser"], message.Text);
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());

            _adminUserService.StopExecutingCommand(message.Chat.Id);
        }

        private async Task OnGetUsers(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["GetUsers"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsToSelectUserComingResource(2)));

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.GettingUsers);
        }

        private async Task OnContinueGettingUsers(ITelegramBotClient botClient, Message message)
        {
            string text;

            UserComingResource? actualComingResource = null;

            if (Enum.TryParse(message.Text, out UserComingResource comingResource))
            {
                actualComingResource = comingResource;
            }

            if (await _chatUserService.DoesUserFromCommitgResourceExist(actualComingResource))
            {
                text = GetChatUsersAsString(await _chatUserService.GetUsersByCommingResource(actualComingResource));
            }
            else
            {
                text = actualComingResource is null
                    ? _resourceReader["DontHaveUsers"]
                    : String.Format(_resourceReader["DontHaveUsersFromResource"], actualComingResource.Value.ToString());
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());

            _adminUserService.StopExecutingCommand(message.Chat.Id);
        }

        private async Task OnGetUsersAsFile(ITelegramBotClient botClient, Message message) 
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["GetUsers"],
                replyMarkup: new ReplyKeyboardMarkup(GetKeyboardsToSelectUserComingResource(2)));

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.GettingUsersAsFile);
        }

        private async Task OnContinueGettingUsersAsFile(ITelegramBotClient botClient, Message message) 
        {
            UserComingResource? actualComingResource = null;

            if (Enum.TryParse(message.Text, out UserComingResource comingResource))
            {
                actualComingResource = comingResource;
            }

            if (!await _chatUserService.DoesUserFromCommitgResourceExist(actualComingResource))
            {
                string text = actualComingResource is null
                    ? _resourceReader["DontHaveUsers"]
                    : String.Format(_resourceReader["DontHaveUsersFromResource"], actualComingResource.Value.ToString());

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: new ReplyKeyboardRemove());
            }
            else 
            {
                using Stream stream = FileCreater.GetUsersAsPDF(await _chatUserService.GetUsersByCommingResource(actualComingResource));

                await botClient.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: new InputOnlineFile(
                        content: stream,
                        fileName: actualComingResource is null
                            ? _resourceReader["NameOfFileWithAllUsersAsPdf"]
                            : String.Format(_resourceReader["NameOfFileWithUsersFromResourceAsPdf"], actualComingResource.Value.ToString())),
                    replyMarkup: new ReplyKeyboardRemove());
            }

            _adminUserService.StopExecutingCommand(message.Chat.Id);
        }

        private async Task OnDoMailingImmediatly(ITelegramBotClient botClient, Message message) 
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["StartDoingMailing"]);

            _adminUserService.StartExecutingCommand(message.Chat.Id, ExecutingCommand.DoingMailingImmediatly);
        }

        private async Task OnContinueDoingMailingImmediatly(ITelegramBotClient botClient, Message message) 
        {
            await _mailService.DoMailingImmediately(message.Text);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: _resourceReader["DoMailingSuccess"]);

            _adminUserService.StopExecutingCommand(message.Chat.Id);
        }

        private async Task OnStartCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
        {
            if (_adminUserService.DoesUserNeedFinishRegistration(admin))
            {
                await _adminUserService.FinishUserRegistration(admin, message.Chat.Id);
            }

            await botClient.SendTextMessageAsync(message.Chat.Id, GetHelpMessage(admin.Role));
        }

        private async Task OnHelpCommand(ITelegramBotClient botClient, Message message, AdminUser admin) 
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

            builder.Append(AdminCommandList.GET_USERS_COMMAND);
            builder.Append(" - ");
            builder.Append(_resourceReader["GetUserDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.GET_USERS_COMMAND_AS_FILE);
            builder.Append(" - ");
            builder.Append(_resourceReader["GetUserAsFileDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.HALPE_COMMAND);
            builder.Append(" - ");
            builder.Append(_resourceReader["HelpDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.DO_MAILING);
            builder.Append(" - ");
            builder.Append(_resourceReader["DoMailingDescription"]);

            builder.Append('\n');
            builder.Append('\n');

            builder.Append(AdminCommandList.DO_MAILING_IMMEDIATLY);
            builder.Append(" - ");
            builder.Append(_resourceReader["DoMailingImmediatlyDescription"]);

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

        private KeyboardButton[][] GetKeyboardsFromAdminUsers(IList<AdminUser> admins, int columns) 
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
        
        private KeyboardButton[][] GetKeyboardsToSelectUserComingResource(int columns) 
        {
            List<string> resources = new(Enum.GetNames<UserComingResource>());

            resources.Add("Get everyone");

            KeyboardButton[][] keyboards;

            int countOfSources = resources.Count();
            int rows = countOfSources / columns;

            if (countOfSources % columns == 0)
            {
                keyboards = new KeyboardButton[rows][];
            }
            else 
            {
                rows++;

                keyboards = new KeyboardButton[rows][];
            }

            for (int i = 0; i < rows; i++)
            {
                KeyboardButton[] buttons;

                if (resources.Skip(i * columns).Count() >= columns)
                {
                    buttons = new KeyboardButton[columns];
                }
                else 
                {
                    buttons = new KeyboardButton[resources.Skip(i * columns).Count()];
                }

                for (int j = 0; j < columns && i * columns + j < countOfSources; j++)
                {
                    buttons[j] = resources[i * columns + j];
                }

                keyboards[i] = buttons;
            }

            return keyboards;
        }

        private string GetChatUsersAsString(IEnumerable<ChatUser> users) 
        {
            StringBuilder stringBuilder = new();

            foreach (ChatUser user in users) 
            {
                stringBuilder.Append(
                    string.Format(_resourceReader["GettingUsersPattern"],
                        user.FirstName, user.Surname, user.UserName, user.From.ToString()));
            }

            return stringBuilder.ToString();
        }

        #endregion
    }
}
