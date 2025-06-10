namespace MindBot.Core
{
    public class Settings
    {
        /// <summary>
        /// Название подключения в appsettings.json
        /// </summary>
        public static string DatabaseConnectionName => "MindTestBotDb";

        /// <summary>
        /// Список ChatId телеграм админов чат-бота
        /// </summary>
        public static List<long> UserIdAdmin => new List<long>() { 6201583180 };

        /// <summary>
        /// Команда запуска/перезапуска бота
        /// </summary>
        public const string CommandBotStart = "/start";

        /// <summary>
        /// Команда запуска тестирования
        /// </summary>
        public const string CommandTestStart = "⚡️Начать тест";
    }
}
