namespace LaboratoryJournal.Models
{
    /// <summary>
    /// Модель записи в лабораторном журнале
    /// </summary>
    public class JournalEntry
    {
        /// <summary>
        /// Уникальный идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID эксперимента
        /// </summary>
        public int ExperimentId { get; set; }

        /// <summary>
        /// ID автора записи
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// Заголовок записи
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Содержание записи
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Тип записи (наблюдение, результат, проблема и т.д.)
        /// </summary>
        public EntryType Type { get; set; }

        /// <summary>
        /// Приоритет записи
        /// </summary>
        public Priority Priority { get; set; } = Priority.Normal;

        /// <summary>
        /// Теги для поиска
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Вложения/ссылки на файлы
        /// </summary>
        public string Attachments { get; set; }

        /// <summary>
        /// Дата и время создания записи
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата и время последнего обновления
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Флаг архивирования
        /// </summary>
        public bool IsArchived { get; set; } = false;

        // Навигационные свойства
        public virtual Experiment Experiment { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }

    /// <summary>
    /// Типы записей в журнале
    /// </summary>
    public enum EntryType
    {
        Observation = 0,        // Наблюдение
        Result = 1,            // Результат
        Problem = 2,           // Проблема
        Note = 3,              // Заметка
        Procedure = 4,         // Процедура
        Conclusion = 5         // Вывод
    }

    /// <summary>
    /// Приоритеты записей
    /// </summary>
    public enum Priority
    {
        Low = 0,        // Низкий
        Normal = 1,     // Обычный
        High = 2,       // Высокий
        Critical = 3    // Критичный
    }
}
