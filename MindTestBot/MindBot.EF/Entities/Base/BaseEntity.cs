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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата обновления записи
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Флаг - запись удалена
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
