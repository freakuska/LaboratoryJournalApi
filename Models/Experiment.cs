namespace LaboratoryJournal.Models
{
    /// <summary>
    /// Модель эксперимента (исследования)
    /// </summary>
    public class Experiment
    {
        /// <summary>
        /// Уникальный идентификатор эксперимента
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название эксперимента
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание эксперимента
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Цель эксперимента
        /// </summary>
        public string Objective { get; set; }

        /// <summary>
        /// Методология проведения
        /// </summary>
        public string Methodology { get; set; }

        /// <summary>
        /// Дата начала эксперимента
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания эксперимента
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Статус эксперимента
        /// </summary>
        public ExperimentStatus Status { get; set; } = ExperimentStatus.Planning;

        /// <summary>
        /// ID исследователя (пользователя)
        /// </summary>
        public string ResearcherId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual ApplicationUser Researcher { get; set; }
        public virtual ICollection<ExperimentResult> Results { get; set; } = new List<ExperimentResult>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }

    /// <summary>
    /// Статусы эксперимента
    /// </summary>
    public enum ExperimentStatus
    {
        Planning = 0,      // Планирование
        InProgress = 1,    // В процессе
        Completed = 2,     // Завершён
        Suspended = 3,     // Приостановлен
        Cancelled = 4      // Отменён
    }
}
