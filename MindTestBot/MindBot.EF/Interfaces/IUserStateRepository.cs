using MindBot.EF.Entities;

namespace MindBot.EF.Interfaces
{
    public interface IUserStateRepository
    {
        /// <summary>
        /// Получить состояние пользователя
        /// </summary>
        /// <param name="chatId">ИД чата телеграм</param>
        /// <returns></returns>
        Task<UserStateEntity?> GetUserState(long chatId);

        /// <summary>
        /// Создание нового состояния пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        Task CreateUserState(long chatId);

        /// <summary>
        /// Обновление состояния пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task UpdateUserState(long chatId, UserStateEntity entity);

        /// <summary>
        /// Удаление состояния пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="forseDelete"></param>
        /// <returns></returns>
        Task DeleteUserState(long chatId, bool forseDelete = false);

        /// <summary>
        /// Сохранение изменений в базе данных
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();

        /// <summary>
        /// Открытие транзакции
        /// </summary>
        /// <returns></returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Подтверждение транзакции
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();

        /// <summary>
        /// Отмена транзакции
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync();
    }
}
