using Telegram.Bot;

using Bot.Services;

//--------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ConfigureBot>();

builder.Services.AddSingleton<ITelegramBotClient>(x => 
    new TelegramBotClient(builder.Configuration.GetSection("BotToken").Value));

var app = builder.Build();

//---------------------------------------

app.MapGet("/", () => "Hello World!");

app.Run();