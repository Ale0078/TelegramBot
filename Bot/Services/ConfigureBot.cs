using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace Bot.Services
{
    public class ConfigureBot : IHostedService
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly IServiceProvider _serviceProvider;

        public ConfigureBot(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _cancellationToken = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

            bot.StartReceiving<BotUpdateHandler>(receiverOptions, _cancellationToken.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationToken.Cancel();

            return Task.CompletedTask;
        }
    }
}
