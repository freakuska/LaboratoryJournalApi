namespace LaboratoryJournal.Models
{
    /// <summary>
    /// Модель результата эксперимента
    /// </summary>
    public class ExperimentResult
    {
        /// <summary>
        /// Уникальный идентификатор результата
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID эксперимента
        /// </summary>
        public int ExperimentId { get; set; }

        /// <summary>
        /// Название результата/измерения
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание результата
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Значение результата
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Единица измерения
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Тип данных результата
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Статус результата
        /// </summary>
        public ResultStatus Status { get; set; } = ResultStatus.Pending;

        /// <summary>
        /// Примечания
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Дата записи результата
        /// </summary>
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual Experiment Experiment { get; set; }
    }

    /// <summary>
    /// Типы данных результатов
    /// </summary>
    public enum DataType
    {
        Numeric = 0,    // Числовое значение
        Text = 1,       // Текстовое значение
        Boolean = 2,    // Логическое значение
        File = 3,       // Файл
        Image = 4,      // Изображение
        Other = 5       // Другое
    }

    /// <summary>
    /// Статусы результата
    /// </summary>
    public enum ResultStatus
    {
        Pending = 0,        // В обработке
        Verified = 1,       // Проверено
        Approved = 2,       // Одобрено
        Rejected = 3,       // Отклонено
        Archived = 4        // Архивировано
    }
}
