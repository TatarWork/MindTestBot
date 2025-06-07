using Telegram.Bot.Types;

namespace MindTestBot.Interfaces
{
    /// <summary>
    /// Интерфейс для реализации бизнес-этапов сценария чат-бота
    /// </summary>
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
        Task SendWelcomeMessageAdminAsync(AppDbContext db, long chatId, string username);

        /// <summary>
        /// Отправка приветствия пользователю чат-бота
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task SendWelcomeMessageAsync(AppDbContext db, long chatId, string username);

        Task HandleStartTestCommand(AppDbContext db, long chatId, string username);

        Task HandleTestAnswerCommand(AppDbContext db, long chatId, string? answer);

        Task SendErrorMessage(long chatId);

        /// <summary>
        /// Генерация результата по итогам формы вопросов
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        Task GenerateAnalysisResultAsync(AppDbContext db, long chatId, string username);

        Task CheckGetBonus(AppDbContext db, long chatId, string username);
    }
}
