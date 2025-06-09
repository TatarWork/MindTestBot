using MindBot.Services.Models;

namespace MindBot.Services.Interfaces
{
    public interface IUserStateService
    {
        /// <summary>
        /// Получение сессии пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task<UserStateModel> GetUserState(long chatId);

        /// <summary>
        /// Запуск новой сессии пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task RestartUserState(long chatId);

        /// <summary>
        /// Обновление сессии пользователя в базе данных
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="userState"></param>
        /// <returns></returns>
        Task UpdateUserState(long chatId, UserStateModel userState);

        /// <summary>
        /// Добавить ответ на вопрос в ходе тестирования
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        Task AddAnswer(long chatId, char answer);

        /// <summary>
        /// Получить прогресс пользователя по ходу теста
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task<(int Current, int Total)> GetUserProgressAsync(long chatId);

        /// <summary>
        /// Проверка - является ли пользователь админом
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        bool CheckIsAdmin(long chatId);
    }
}
