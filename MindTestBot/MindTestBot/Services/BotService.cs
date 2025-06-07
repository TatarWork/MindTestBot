using MindTestBot.Interfaces;
using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using MindTestBot.Helpers;
using MindTestBot.Extensions;

namespace MindTestBot.Services
{
    public class BotService : IBotService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IScriptService _scriptService;
        private readonly Type _thisType;

        public BotService(IScriptService scriptService,
            ILogger<BotService> logger)
        {
            _logger = logger;
            _scriptService = scriptService;
            _thisType = GetType();
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(errorMessage);

            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            try
            {
                switch (update)
                {
                    case { Message: { } message }:
                        await _scriptService.HandleMessageAsync(message);
                        break;

                    case { CallbackQuery: { } callbackQuery }:
                        await _scriptService.HandleCallbackQueryAsync(callbackQuery);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during operation {LogHelper.GetMethodName(_thisType, nameof(HandleUpdateAsync))}: {ex.GetFullException()}");
            }
        }
    }
}
