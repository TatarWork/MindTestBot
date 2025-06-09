namespace MindBot.Core.Enums
{
    /// <summary>
    /// Перечисление состояний пользователя
    /// </summary>
    public enum UserStateEnum
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        None = 0,

        /// <summary>
        /// Приветственное сообщение
        /// </summary>
        WelcomeMessage = 1,

        /// <summary>
        /// Прохождение теста
        /// </summary>
        Questions = 2,

        /// <summary>
        /// Получен резльутат тестирования
        /// </summary>
        Result = 3,

        /// <summary>
        /// Получен бонус - консультация
        /// </summary>
        SendBonusConsult = 4,

        /// <summary>
        /// Получен бонус - ссылка на закрытый телеграм-канал
        /// </summary>
        SendBonusVipChannel = 5,
    }
}
