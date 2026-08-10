using Microsoft.AspNetCore.Identity;

namespace LaboratoryJournal.Models
{
    /// <summary>
    /// Расширенная модель пользователя с дополнительными свойствами
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Полное имя пользователя
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Должность пользователя
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Дата создания учётной записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего входа
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Статус активности учётной записи
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}
