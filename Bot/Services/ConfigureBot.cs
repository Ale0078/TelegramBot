using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Services
{
    public class ConfigureBot : IHostedService
    {
        private readonly int _timeOfMalingByUa;
        private readonly IServiceProvider _serviceProvider;

        private Timer _timer;

        public ConfigureBot(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _timeOfMalingByUa = 19;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            IConfiguration configuration = scope.ServiceProvider.GetService<IConfiguration>();
            ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();

            BotUpdateHandler updateHandler = scope.ServiceProvider.GetService<BotUpdateHandler>();
            AdminBotUpdateHandler adminBotUpdate = scope.ServiceProvider.GetService<AdminBotUpdateHandler>();

            ITelegramBotClient adminBot = new TelegramBotClient(configuration.GetSection("AdminBotToken").Value);

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

            bot.StartReceiving(
                updateHandler: updateHandler.HandleUpdateAsync,
                errorHandler: updateHandler.HandleErrorAsync,
                receiverOptions: receiverOptions, 
                cancellationToken: cancellationToken);

            adminBot.StartReceiving(
                updateHandler: adminBotUpdate.HandleUpdateAsync,
                errorHandler: adminBotUpdate.HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken);

            SetTimer();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void SetTimer() 
        {
            TimeSpan dueTime;
            TimeSpan period = new(24, 0, 0);

            DateTime currentTime = DateTime.UtcNow.AddHours(2);
            
            if (currentTime.Hour < _timeOfMalingByUa)
            {
                DateTime timeToMail = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, _timeOfMalingByUa, 0, 0);

                dueTime = timeToMail.Subtract(currentTime);
            }
            else 
            {
                DateTime timeToMail = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day + 1, _timeOfMalingByUa, 0, 0);

                dueTime = timeToMail.Subtract(currentTime);
            }

            _timer = new Timer(
                callback: OnTimerCallBack,
                state: null,
                dueTime: dueTime,
                period: period);
        }

        private async void OnTimerCallBack(object _) 
        {
            ITelegramBotClient bot = _serviceProvider.GetService<ITelegramBotClient>();
            ChatService chatService = _serviceProvider.GetService<ChatService>();
            ResourceReader resourceReader = _serviceProvider.GetService<ResourceReader>();

            InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton(resourceReader["LinkAnderSecondMailing"])
            {
                Url = resourceReader["UriToManager"]
            });

            foreach (Models.Chat chat in chatService.GetAllUnmaledUserThatFinishedTest())
            {
                await bot.SendTextMessageAsync(chat.Id, resourceReader["FirstMailing"]);

                await bot.SendTextMessageAsync(chat.Id, resourceReader["SecondMailing"], replyMarkup: keyboard);
            }

            chatService.MailAllUnmaledChats();
        }
    }
}
