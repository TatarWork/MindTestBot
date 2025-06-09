using System.ComponentModel.DataAnnotations;

namespace MindBot.Services.Models
{
    public class QuestionModel
    {
        public required string Text { get; set; }

        /// <summary>
        /// Варианты ответов храним как JSON
        /// </summary>
        [Required]
        public string OptionsJson { get; set; } = "{}";

        /// <summary>
        /// Порядок вопроса
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Свойство для работы с вариантами (не сохраняется в БД)
        /// </summary>
        public Dictionary<char, string> Options
        {
            get => System.Text.Json.JsonSerializer.Deserialize<Dictionary<char, string>>(OptionsJson)!;
            set => OptionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}
