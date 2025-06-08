using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MindBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MindBot.Services.BackgroundServices
{
    public class BotBackgroundService : BackgroundService
    {
        private readonly ILogger<BotBackgroundService> _logger;
        private readonly ITelegramBotClient _bot;
        private readonly IBotService _botServcie;

        public BotBackgroundService(
            ILogger<BotBackgroundService> logger,
            ITelegramBotClient bot,
            IBotService botService)
        {
            _logger = logger;
            _bot = bot;
            _botServcie = botService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting bot background service");

            await _bot.SetMyCommands(
                new List<BotCommand>
                {
                    new() { Command = "start", Description = "Старт" },
                });

            _bot.StartReceiving(
                updateHandler: _botServcie.HandleUpdateAsync,
                errorHandler: _botServcie.HandlePollingErrorAsync,
                receiverOptions: new()
                {
                    DropPendingUpdates = true,
                    AllowedUpdates = Array.Empty<UpdateType>()
                },
                cancellationToken: stoppingToken
            );

            var me = await _bot.GetMe(stoppingToken);

            _logger.LogInformation("Bot {BotName} started (@{BotUsername})", me.FirstName, me.Username);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
