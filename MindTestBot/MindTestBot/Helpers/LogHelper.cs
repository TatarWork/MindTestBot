namespace MindTestBot.Helpers
{
    public static class LogHelper
    {
        /// <summary>
        /// Получение полного пути к методу (для логирования)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static string GetMethodName(Type type, string methodName)
        {
            if (type == null || methodName == null)
                return string.Empty;

            return $"{type.FullName}.{methodName}";
        }
    }
}
