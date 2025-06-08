using MindBot.Core.Enums;

namespace MindBot.Core.Extensions
{
    public static class EnumExtension
    {
        public static string ToCodeValue(this SystemEnum value)
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
                case SystemEnum.WelcomeMessageUser:
                    return @"я Дарья Татар, маркетолог и эксперт созданию продуктов, спасибо что откликнулись и выделили время на прохождение теста, в конце вас ждет подарок на выбор от меня, тест займет не более 10 минут вашего времени";
                case SystemEnum.WelcomeMessageAdmin:
                    return "Это чат-бот Дарьи Татар, сюда будут приходить уведомления о пользователях прошедших тестирование";
                case SystemEnum.StartTesting:
                    return "Начать тест";
                case SystemEnum.None:
                default:
                    return "Неизвестно";
            }
        }

        public static string ToStringValue(this BonusTypeEnum value)
        {
            switch (value)
            {
                case BonusTypeEnum.Consulting:
                    return "";
                case BonusTypeEnum.VipChannel:
                    return "";
                case BonusTypeEnum.None:
                default:
                    return "Неизвестно";
            }
        }
    }
}
