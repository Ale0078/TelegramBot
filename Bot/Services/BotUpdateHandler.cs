﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

using Bot.Models;
using Bot.Data;

namespace Bot.Services
{
    //Only to private chat
    public class BotUpdateHandler : IUpdateHandler
    {
        private const string USER_ID = "UserId";

        private readonly SynchronizedCollection<long> _chats;
        
        private readonly ResourceReader _resourceReader;
        private readonly TestExecutor _testExecutor;
        private readonly ChatService _chatService;

        public BotUpdateHandler(ResourceReader resourceReader, TestExecutor testExecutor, ChatService chatService)
        {
            _chats = new SynchronizedCollection<long>();

            _resourceReader = resourceReader;
            _testExecutor = testExecutor;
            _chatService = chatService;
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: (long)exception.Data[USER_ID],
                text: _resourceReader["ExceptionMessage"]);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(botClient, update.Message),
                UpdateType.CallbackQuery => OnCallbackQueryReceive(botClient, update.CallbackQuery),
                UpdateType.MyChatMember => OnMyCharMemberStateReceive(botClient, update.MyChatMember)
            };

            try
            {
                await handler;
            }
            catch (Exception ex) 
            {
                ex.Data.Add(USER_ID, update.Message.Chat.Id);

                await HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        private Task OnMyCharMemberStateReceive(ITelegramBotClient botClient, ChatMemberUpdated myChatMember) 
        {
            return myChatMember.NewChatMember.Status switch
            {
                ChatMemberStatus.Kicked => RemoveUser(myChatMember.From.Id),
                _ => GetDefualtTask()
            };
        }

        private Task RemoveUser(long userId) 
        {
            if (_testExecutor.HasUser(userId))
            {
                _testExecutor.RemoveUser(userId);
            }

            if (_chatService.DoesExist(userId))
            {
                _chatService.RemoveChat(userId);
            }

            return Task.CompletedTask;
        }

        private Task GetDefualtTask() => Task.CompletedTask;

        private Task OnCallbackQueryReceive(ITelegramBotClient botClient, CallbackQuery callbackQuery) 
        {
            return callbackQuery.Data switch
            {
                "/trail" => GetTrail(botClient, callbackQuery.Message.Chat.Id),
                "/checkList" => GetCheckList(botClient, callbackQuery.Message.Chat.Id),
                "/wantEverything" => GetTrailAndCheckList(botClient, callbackQuery.Message.Chat.Id)
            };
        }

        private async Task OnMessageReceived(ITelegramBotClient botClient, Message message) 
        {
            if (message.Type is not MessageType.Text)
            {
                return;
            }

            await ChooseAction(botClient, message);
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
                    "/start" => StartBot(botClient, message.Chat.Id),
                    _ => ContinueStart(botClient, message)
                };
            }

            return action;
        }

        private async Task StartBot(ITelegramBotClient botClient, long chatId)
        {
            ReplyKeyboardMarkup keyboard = new(new[]
            {
                new[] { new KeyboardButton(UserComingResource.Instagram.ToString()), new KeyboardButton(UserComingResource.TikTok.ToString()) },
                new[] { new KeyboardButton(UserComingResource.Reels.ToString()), new KeyboardButton(UserComingResource.Another.ToString()) }
            });

            await botClient.SendTextMessageAsync(
                chatId: chatId, 
                text:_resourceReader["FromUserResource"],
                replyMarkup: keyboard);

            if (!_chats.Contains(chatId))
            {
                _chats.Add(chatId);
            }
        }

        private async Task ContinueStart(ITelegramBotClient botClient, Message message) 
        {
            if (Enum.TryParse(message.Text, out UserComingResource resource) && _chats.Contains(message.Chat.Id))
            {
                if (!_chatService.DoesExist(message.Chat.Id))
                {
                    await _chatService.AddChat(message.Chat.Id, new ChatUser
                    {
                        FirstName = message.From.FirstName is null 
                            ? "" 
                            : message.From.FirstName,
                        Surname = message.From.LastName is null
                            ? ""
                            : message.From.LastName,
                        UserName = message.From.Username,
                        From = resource
                    });
                }

                _chats.Remove(message.Chat.Id);

                await StartTest(botClient, message.Chat.Id);
            }
            else 
            {
                throw new Exception();
            }
        }

        private async Task StartTest(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, _resourceReader["TestIsStarted"]);

            _testExecutor.AddUser(chatId);

            await PrepairQuestionToUser(botClient, chatId);
        }

        private async Task GetTrail(ITelegramBotClient botClient, long chatId) 
        {
            await botClient.SendTextMessageAsync(chatId, _resourceReader["TrailDiscription"]);
        }

        private async Task GetCheckList(ITelegramBotClient botClient, long chatId) 
        {
            await botClient.SendTextMessageAsync(chatId, _resourceReader["CheckListDiscription"]);
        }

        private async Task GetTrailAndCheckList(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, _resourceReader["WantEverythingDiscription"]);
        }

        private async Task ContinuTest(ITelegramBotClient botClient, Message message) 
        {
            string result = _testExecutor.CheckAnswer(message.Chat.Id, message.Text);

            await botClient.SendTextMessageAsync(message.Chat.Id, result);

            if (_testExecutor.DoesUserFinishTest(message.Chat.Id))
            {
                await FinishTest(botClient, message.Chat.Id);
            }
            else 
            {
                await PrepairQuestionToUser(botClient, message.Chat.Id);
            }
        }

        private async Task FinishTest(ITelegramBotClient botClient, long chatId) 
        {
            string finalResult = _testExecutor.FinishTest(chatId);

            await botClient.SendTextMessageAsync(
                chatId: chatId, 
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
                chatId: chatId, 
                text: _resourceReader["FinishTest"], 
                replyMarkup: keyboard);

            await _chatService.FinishTest(chatId);
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
