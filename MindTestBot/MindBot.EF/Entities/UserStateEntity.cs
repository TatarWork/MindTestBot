using MindBot.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindBot.EF.Entities
{
    /// <summary>
    /// Сущность для работы с ответами пользователей
    /// </summary>
    public class UserStateEntity
    {
        // <summary>
        /// ИД записи
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата обновления записи
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Флаг - запись удалена
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// ИД чата пользователя в Телеграм
        /// </summary>
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
    }
}
