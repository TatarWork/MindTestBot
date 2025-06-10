using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindBot.EF.Entities.Base
{
    /// <summary>
    /// Базовая сущность системы
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// ИД записи
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата обновления записи
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Флаг - запись удалена
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
