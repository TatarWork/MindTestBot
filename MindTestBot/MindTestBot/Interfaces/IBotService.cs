using Telegram.Bot;
using Telegram.Bot.Types;

namespace MindTestBot.Interfaces
{
    public interface IBotService
    {
        /// <summary>
        /// Обработчик уведомлений от системы Telegram серверу
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="update"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct);

        /// <summary>
        /// Обработчик уведомлений об ошибках системы Telegram
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="exception"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct);
    }
}
