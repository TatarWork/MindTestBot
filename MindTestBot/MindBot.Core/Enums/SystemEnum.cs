namespace MindBot.Core.Enums
{
    /// <summary>
    /// Перечисление для управления бизнес-логикой приложения
    /// </summary>
    public enum SystemEnum
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        None = 0,

        /// <summary>
        /// Приветственное сообщение пользователя
        /// </summary>
        WelcomeMessageUser = 100,

        /// <summary>
        /// Приветственное сообщение администратора
        /// </summary>
        WelcomeMessageAdmin = 101,

        /// <summary>
        /// Начало тестирования
        /// </summary>
        StartTesting = 200,

        /// <summary>
        /// Вопрос теста 1
        /// </summary>
        Question_1 = 1,

        /// <summary>
        /// Вопрос теста 2
        /// </summary>
        Question_2 = 2,

        /// <summary>
        /// Вопрос теста 3
        /// </summary>
        Question_3 = 3,

        /// <summary>
        /// Вопрос теста 4
        /// </summary>
        Question_4 = 4,

        /// <summary>
        /// Вопрос теста 5
        /// </summary>
        Question_5 = 5,

        /// <summary>
        /// Вопрос теста 6
        /// </summary>
        Question_6 = 6,

        /// <summary>
        /// Вопрос теста 7
        /// </summary>
        Question_7 = 7,

        /// <summary>
        /// Вопрос теста 8
        /// </summary>
        Question_8 = 8,

        /// <summary>
        /// Получение результата
        /// </summary>
        SendResultTesting = 201,

        /// <summary>
        /// Получен бонус - пользователь хочет беслпатную 15ти минутную консультацию
        /// </summary>
        SendBonusConsulting = 300,

        /// <summary>
        /// Получен бонус - пользователь хочет присоединиться к закрытому каналу Телеграм
        /// </summary>
        SendBonusVipChannel = 301,
    }
}
