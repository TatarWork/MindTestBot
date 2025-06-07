using MindTestBot.Enums;

namespace MindTestBot.Extensions
{
    public static class EnumExtension
    {
        public static string ToCodeValue(this SystemEnum value)
        {
            return value.ToString();
        }

        public static string ToCodeValue(this ResultBonusEnum value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Преобразует строковый код перечисления к типизированному элементу перечисления
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetEnumFromCode<T>(this string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), (string)value, true);
        }

        /// <summary>
        /// Строкове представление настроек чат-бота
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringValue(this SystemEnum value)
        {
            switch (value)
            {              
                default:
                    return "Неизвестно";
            }
        }
    }
}
