using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

using Bot.Models;

namespace Bot.Services
{
    public class BotUpdateHandler : IUpdateHandler
    {
        private readonly ResourceReader _resourceReader;
        private readonly TestExecutor _testExecutor;

        public BotUpdateHandler(ResourceReader resourceReader, TestExecutor testExecutor)
        {
            _resourceReader = resourceReader;
            _testExecutor = testExecutor;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(botClient, update),
                UpdateType.CallbackQuery => OnCallbackQueryReceive(botClient, update),
                UpdateType.MyChatMember => A()
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

        private Task A() => Task.CompletedTask;

        private Task OnCallbackQueryReceive(ITelegramBotClient botClient, Update update) 
        {
            return update.CallbackQuery.Data switch
            {
                "/trail" => GetTrail(botClient, update.CallbackQuery.Message),
                "/checkList" => GetCheckList(botClient, update.CallbackQuery.Message),
                "/wantEverything" => GetTrailAndCheckList(botClient, update.CallbackQuery.Message)
            };
        }

        private async Task OnMessageReceived(ITelegramBotClient botClient, Update update) 
        {
            if (update.Message.Type is not MessageType.Text)
            {
                return;
            }

            await ChooseAction(botClient, update.Message);
        }

        private Task ChooseAction(ITelegramBotClient botClient, Message message) 
        {
            Task action;

            if (_testExecutor.HasUser(message.Chat.Id))
            {
                action = ContinuTest(botClient, message);
            }
            else
            {
                action = message.Text switch
                {
                    "/start" => Start(botClient, message)
                };
            }

            return action;
        }

        private async Task Start(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["TestIsStarted"]);

            _testExecutor.AddUser(message.Chat.Id);

            await PrepairQuestionToUser(botClient, message.Chat.Id);
        }

        private async Task GetTrail(ITelegramBotClient botClient, Message message) 
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["TrailDiscription"]);
        }

        private async Task GetCheckList(ITelegramBotClient botClient, Message message) 
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["CheckListDiscription"]);
        }

        private async Task GetTrailAndCheckList(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, _resourceReader["WantEverythingDiscription"]);
        }

        private async Task ContinuTest(ITelegramBotClient botClient, Message message) 
        {
            string result = _testExecutor.CheckAnswer(message.Chat.Id, message.Text);

            await botClient.SendTextMessageAsync(message.Chat.Id, result);

            if (_testExecutor.DoesUserFinishTest(message.Chat.Id))
            {
                await FinishTest(botClient, message);
            }
            else 
            {
                await PrepairQuestionToUser(botClient, message.Chat.Id);
            }
        }

        private async Task FinishTest(ITelegramBotClient botClient, Message message) 
        {
            string finalResult = _testExecutor.FinishTest(message.Chat.Id);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, 
                text: finalResult, 
                replyMarkup: new ReplyKeyboardRemove());

            InlineKeyboardMarkup keyboard = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_resourceReader["Trail"], "/trail"),
                    InlineKeyboardButton.WithCallbackData(_resourceReader["CheckList"], "/checkList"),
                    InlineKeyboardButton.WithCallbackData(_resourceReader["WantEverything"], "/wantEverything")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, 
                text: _resourceReader["FinishTest"], 
                replyMarkup: keyboard);
        }

        private async Task PrepairQuestionToUser(ITelegramBotClient botClient, long userId) 
        {
            Question question = _testExecutor.GetCurrentQuestion(userId);

            ReplyKeyboardMarkup keyboard = new(GenerateKeyboardToQuestion(question, 1));

            await botClient.SendTextMessageAsync(
                chatId: userId,
                text: question.Content,
                replyMarkup: keyboard);
        }

        private KeyboardButton[][] GenerateKeyboardToQuestion(Question question, int columnInRow)
        {
            KeyboardButton[][] keyboards = new KeyboardButton[question.Answers.Count][];

            for (int i = 0; i < question.Answers.Count; i++) 
            {
                KeyboardButton[] buttons;

                if (question.Answers.Skip(i * columnInRow).Count() >= columnInRow)
                {
                    buttons = new KeyboardButton[columnInRow];
                }
                else 
                {
                    buttons = new KeyboardButton[question.Answers.Skip(i * columnInRow).Count()];
                }

                for (int j = 0; j < columnInRow && i * columnInRow + j < question.Answers.Count; j++) 
                {
                    buttons[j] = question.Answers[i * columnInRow + j].Content;
                }

                keyboards[i] = buttons;
            }

            return keyboards;
        }
    }
}
