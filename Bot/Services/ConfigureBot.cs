using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Bot.Services
{
    public class ConfigureBot : IHostedService
    {
        //private readonly CancellationTokenSource _cancellationToken;
        //private readonly IServiceProvider _serviceProvider;

        //public ConfigureBot(IServiceProvider serviceProvider)
        //{
        //    _serviceProvider = serviceProvider;

        //    _cancellationToken = new CancellationTokenSource();
        //}

        //public Task StartAsync(CancellationToken cancellationToken)
        //{
        //    using var scope = _serviceProvider.CreateScope();

        //    ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();

        //    ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

        //    bot.StartReceiving<BotUpdateHandler>(receiverOptions, _cancellationToken.Token);

        //    return Task.CompletedTask;
        //}

        //public Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _cancellationToken.Cancel();

        //    return Task.CompletedTask;
        //}
        private readonly IServiceProvider _serviceProvider;
        private readonly BotConfiguration _botConfiguration;

        public ConfigureBot(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            _botConfiguration = configuration.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();

            string webhookAddress = $"{_botConfiguration.HostAddress}/bot/{_botConfiguration.Token}";

            await bot.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            ITelegramBotClient bot = scope.ServiceProvider.GetService<ITelegramBotClient>();

            await bot.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
