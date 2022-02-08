using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

using Bot;
using Bot.Entities;
using Bot.Services;

//--------------------------------------

var builder = WebApplication.CreateBuilder(args);

BotConfiguration botConfiguration = builder.Configuration.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>();

builder.Services.AddHostedService<ConfigureBot>();

//builder.Services.AddScoped<ITelegramBotClient>(x => 
//    new TelegramBotClient(builder.Configuration.GetSection("BotToken").Value));
builder.Services.AddScoped<TelegramBotUpdateHandler>();

builder.Services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(
        token: botConfiguration.Token,
        httpClient: httpClient));

builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(
    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serverVersion: new MySqlServerVersion(new Version(builder.Configuration.GetSection("MySqlServerVersion").Value))));

builder.Services.AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();

//---------------------------------------

app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "tgwebhook",
        pattern: $"bot/{botConfiguration.Token}",
        new { controller = "Bot", action = "Post" });

    endpoints.MapControllers();
});

app.Run();