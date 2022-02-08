using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

using Bot.Services;

namespace Bot.Controllers
{
    public class BotController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] TelegramBotUpdateHandler handler, [FromBody] Update update) 
        {
            await handler.HandleUpdateAsync(update);

            return Ok();
        }
    }
}
