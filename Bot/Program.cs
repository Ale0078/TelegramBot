using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Telegram.Bot;

using Bot;
using Bot.AutoMapperProfiles;
using Bot.Entities;
using Bot.Services;

//--------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ConfigureBot>();

builder.Services.AddTransient<ChatService>(_ => new ChatService(CreateContext()));
builder.Services.AddTransient<TestExecutor>(serviceProvider => new TestExecutor(CreateContext(), 
    serviceProvider.GetService<IMapper>(), serviceProvider.GetService<ResourceReader>()));
builder.Services.AddTransient<BotUpdateHandler>();

builder.Services.AddSingleton<ResourceReader>(x => new ResourceReader(builder.Configuration.GetSection("ResourcePath").Value));
builder.Services.AddSingleton<ITelegramBotClient>(x =>
    new TelegramBotClient(builder.Configuration.GetSection("BotToken").Value));

builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        serverVersion: new MySqlServerVersion(new Version(builder.Configuration.GetSection("MySqlServerVersion").Value))));

builder.Services.AddAutoMapper(typeof(SuccessMessageProfile), typeof(FailMessageProfile), typeof(AnswerProfile), typeof(QuestionProfile));

var app = builder.Build();

//---------------------------------------

app.Run();

//---------------------------------------

ApplicationContext CreateContext() 
{
    var options = new DbContextOptionsBuilder<ApplicationContext>();

    options.UseMySql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        serverVersion: new MySqlServerVersion(new Version(builder.Configuration.GetSection("MySqlServerVersion").Value)));

    return new ApplicationContext(options.Options);
}