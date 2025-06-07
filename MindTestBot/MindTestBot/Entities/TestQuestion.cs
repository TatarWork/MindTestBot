using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MindTestBot.Entities
{
    /// <summary>
    /// Сущность для работы 
    /// </summary>
    public class TestQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Text { get; set; } = null!;

        /// <summary>
        /// Варианты ответов храним как JSON
        /// </summary>
        [Required]
        public string OptionsJson { get; set; } = "{}";

        /// <summary>
        /// Порядок вопроса
        /// </summary>
        [Required]
        public int Order { get; set; }

        /// <summary>
        /// Флаг - вопрос активен
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Свойство для работы с вариантами (не сохраняется в БД)
        /// </summary>
        [NotMapped]
        public Dictionary<char, string> Options
        {
            get => System.Text.Json.JsonSerializer.Deserialize<Dictionary<char, string>>(OptionsJson)!;
            set => OptionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Метод для добавления варианта ответа
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOption(char key, string value)
        {
            var options = Options;
            options[key] = value;
            Options = options;
        }
    }
}
