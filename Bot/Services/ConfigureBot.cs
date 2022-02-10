using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace Bot.Services
{
    public class ConfigureBot : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ConfigureBot(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();
            BotUpdateHandler updateHandler = scope.ServiceProvider.GetService<BotUpdateHandler>();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

            bot.StartReceiving(
                updateHandler: updateHandler.HandleUpdateAsync,
                errorHandler: updateHandler.HandleErrorAsync,
                receiverOptions: receiverOptions, 
                cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
