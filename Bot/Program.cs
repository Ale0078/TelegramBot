using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

using Bot;
using Bot.Entities;
using Bot.Services;

//--------------------------------------

var builder = WebApplication.CreateBuilder(args);

BotConfiguration botConfiguration = builder.Configuration.GetSection(nameof(BotConfiguration)).Get<BotConfiguration>();

builder.Services.AddHostedService<ConfigureBot>();

builder.Services.AddScoped<BotUpdateHandler>();
builder.Services.AddScoped<ResourceReader>(x => new ResourceReader(builder.Configuration.GetSection("ResourcePath").Value));
builder.Services.AddScoped<ITelegramBotClient>(x =>
    new TelegramBotClient(builder.Configuration.GetSection("BotToken").Value));

builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(
    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    serverVersion: new MySqlServerVersion(new Version(builder.Configuration.GetSection("MySqlServerVersion").Value))));

var app = builder.Build();

//---------------------------------------

app.Run();