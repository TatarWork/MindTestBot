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
        /// Отправка приветствия администратору чат-бота
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task SendWelcomeMessageAdminAsync(long chatId, string username);

        /// <summary>
        /// Отправка приветствия пользователю чат-бота
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task SendWelcomeMessageAsync(long chatId, string username);

        Task HandleStartTestCommand(long chatId, string username);

        Task HandleTestAnswerCommand(long chatId, string? answer);

        Task SendErrorMessage(long chatId);

        /// <summary>
        /// Генерация результата по итогам формы вопросов
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        Task GenerateAnalysisResultAsync(long chatId, string username);

        Task CheckGetBonus(long chatId, string username);
    }
}
