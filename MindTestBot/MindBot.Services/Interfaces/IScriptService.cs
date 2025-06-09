using Telegram.Bot.Types;

namespace MindBot.Services.Interfaces
{
    public interface IScriptService
    {
        /// <summary>
        /// Получение сообщений от пользователя по каналу Telegram
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task HandleMessageAsync(Message message);

        /// <summary>
        /// Получение команды "нажатия" пользователем кнопки в чат-боте Telegram
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        Task HandleCallbackQueryAsync(CallbackQuery callbackQuery);

        /// <summary>
        /// Команда приветствия, запускается при старте/перезапуске чат-бота
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        Task SendCommandWelcome(long chatId, string username, string? firstName = null, string? lastName = null);

        /// <summary>
        /// Отправка сообщения-приветствия пользователю
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        Task SendMessageWelcomeUser(long chatId, string username);

        /// <summary>
        /// Отправка сообщения-приветствия админу
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        Task SendMessageWelcomeAdmin(long chatId, string username);

        /// <summary>
        /// Команда запуска тестирования пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        Task SendCommandTestStart(long chatId, string username);

        /// <summary>
        /// Команда обработки ответов в ходе тестирования пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        Task SendCommandTestAnswer(long chatId, string? answer);
    }
}
