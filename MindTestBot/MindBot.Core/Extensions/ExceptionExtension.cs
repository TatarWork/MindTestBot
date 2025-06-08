namespace MindBot.Core.Extensions
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// Получение полного текста ошибки
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string GetFullException(this Exception exception)
        {
            var stack = exception.StackTrace;
            var messages = exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => $"Type:{ex.GetType().FullName}:{ex.Message}");
            return $"Exception type:{exception.GetType().FullName}{Environment.NewLine}{string.Join(Environment.NewLine, messages)}{Environment.NewLine}{stack}";
        }

        private static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        private static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }
    }
}
