using Microsoft.EntityFrameworkCore;
using MindBot.Core.Enums;
using MindBot.EF.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindBot.EF.Entities
{
    /// <summary>
    /// Сущность для работы с ответами пользователей
    /// </summary>
    [Index(nameof(ChatId), IsUnique = true)]
    public class UserStateEntity : BaseEntity
    {
        /// <summary>
        /// ИД чата пользователя в Телеграм
        /// </summary>
        [Key]
        public long ChatId { get; set; }

        /// <summary>
        /// Текущий вопрос
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Номер вопроса должен быть от 1 до 100")]
        public int CurrentQuestion { get; set; } = 1;

        /// <summary>
        /// Список ответов хранящийся как JSON
        /// </summary>
        [Column(nameof(AnswersJson), TypeName = "jsonb")]
        [Required]
        public string AnswersJson { get; set; } = "[]";

        /// <summary>
        /// Флаг - тестирование завершено
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Флаг - бонус получен
        /// </summary>
        public bool IsGetBonus { get; set; }

        /// <summary>
        /// Результат тестирования
        /// </summary>
        [MaxLength(500)]
        public string? Result { get; set; }

        /// <summary>
        /// Имя клиента
        /// </summary>
        [MaxLength(50)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Фамилия клиента
        /// </summary>
        [MaxLength(50)]
        public string? LastName { get; set; }

        /// <summary>
        /// Имя пользователя в системе телеграм
        /// </summary>
        [MaxLength(50)]
        public string? Username { get; set; }

        /// <summary>
        /// Состояние польователя
        /// </summary>
        public UserStateEnum State { get; set; }

        /// <summary>
        /// Свойство для работы с ответами (не сохраняется в БД)
        /// </summary>
        [NotMapped]
        public List<char> Answers
        {
            get => System.Text.Json.JsonSerializer.Deserialize<List<char>>(AnswersJson)!;
            set => AnswersJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Метод добавления ответа пользователя
        /// </summary>
        /// <param name="answer"></param>
        public void AddAnswer(char answer)
        {
            var answers = Answers;

            if (answers == null)
                answers = new List<char>();

            answers.Add(answer);

            Answers = answers;
        }
    }
}
