using Telegram.Bot;

using Bot.Services;

//--------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ConfigureBot>();

builder.Services.AddSingleton<ITelegramBotClient>(x => 
    new TelegramBotClient(builder.Configuration.GetSection("BotToken").Value));

builder.Services.AddControllers();

var app = builder.Build();

//---------------------------------------

app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();