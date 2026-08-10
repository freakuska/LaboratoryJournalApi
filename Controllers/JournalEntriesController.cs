using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaboratoryJournal.Data;
using LaboratoryJournal.Models;

namespace LaboratoryJournal.Controllers
{
    /// <summary>
    /// Контроллер управления записями в лабораторном журнале
    /// </summary>
    [ApiController]
    [Route("api/journal-entries")]
    [Authorize]
    public class JournalEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JournalEntriesController> _logger;

        public JournalEntriesController(ApplicationDbContext context, ILogger<JournalEntriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить записи для конкретного эксперимента
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEntries(
            [FromQuery] string searchTerm = "",
            [FromQuery] EntryType? entryType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = _context.JournalEntries
                .Where(j => j.Experiment.ResearcherId == userId && !j.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(j =>
                    j.Title.Contains(searchTerm) ||
                    j.Content.Contains(searchTerm) ||
                    j.Tags.Contains(searchTerm));
            }

            if (entryType.HasValue)
            {
                query = query.Where(j => j.Type == entryType.Value);
            }

            var total = await query.CountAsync();
            var entries = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.ExperimentId,
                    j.Title,
                    j.Content,
                    j.Type,
                    j.Priority,
                    j.Tags,
                    j.CreatedAt,
                    j.UpdatedAt,
                    Author = j.Author.FullName
                })
                .ToListAsync();

            return Ok(new { total, pageNumber, pageSize, data = entries });
        }

        [HttpGet("experiment/{experimentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEntriesByExperiment(
            int experimentId,
            [FromQuery] string searchTerm = "",
            [FromQuery] EntryType? entryType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Проверка, что эксперимент принадлежит пользователю
            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == experimentId && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            var query = _context.JournalEntries
                .Where(j => j.ExperimentId == experimentId && !j.IsArchived)
                .AsQueryable();

            // Поиск по содержанию
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(j =>
                    j.Title.Contains(searchTerm) ||
                    j.Content.Contains(searchTerm) ||
                    j.Tags.Contains(searchTerm));
            }

            // Фильтрация по типу записи
            if (entryType.HasValue)
            {
                query = query.Where(j => j.Type == entryType.Value);
            }

            var total = await query.CountAsync();
            var entries = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Content,
                    j.Type,
                    j.Priority,
                    j.Tags,
                    j.CreatedAt,
                    j.UpdatedAt,
                    Author = j.Author.FullName
                })
                .ToListAsync();

            return Ok(new { total, pageNumber, pageSize, data = entries });
        }

        /// <summary>
        /// Получить конкретную запись
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEntry(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var entry = await _context.JournalEntries
                .Include(j => j.Experiment)
                .Include(j => j.Author)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (entry == null)
                return NotFound(new { message = "Запись не найдена" });

            // Проверка доступа
            if (entry.Experiment.ResearcherId != userId && entry.AuthorId != userId)
                return Forbid();

            return Ok(new
            {
                entry.Id,
                entry.Title,
                entry.Content,
                entry.Type,
                entry.Priority,
                entry.Tags,
                entry.Attachments,
                entry.CreatedAt,
                entry.UpdatedAt,
                Author = entry.Author.FullName,
                Experiment = entry.Experiment.Title
            });
        }

        /// <summary>
        /// Создать новую запись в журнале
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateEntry([FromBody] CreateJournalEntryRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Проверка, что эксперимент существует
            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == request.ExperimentId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            var entry = new JournalEntry
            {
                ExperimentId = request.ExperimentId,
                AuthorId = userId,
                Title = request.Title,
                Content = request.Content,
                Type = request.Type,
                Priority = request.Priority ?? Priority.Normal,
                Tags = request.Tags,
                Attachments = request.Attachments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsArchived = false
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Создана новая запись в журнале: {entry.Id}");

            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, new
            {
                entry.Id,
                entry.Title,
                entry.Type,
                entry.CreatedAt
            });
        }

        /// <summary>
        /// Обновить запись в журнале
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(int id, [FromBody] UpdateJournalEntryRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == id);

            if (entry == null)
                return NotFound(new { message = "Запись не найдена" });

            // Только автор или администратор может редактировать
            if (entry.AuthorId != userId)
                return Forbid();

            entry.Title = request.Title ?? entry.Title;
            entry.Content = request.Content ?? entry.Content;
            entry.Type = request.Type ?? entry.Type;
            entry.Priority = request.Priority ?? entry.Priority;
            entry.Tags = request.Tags ?? entry.Tags;
            entry.Attachments = request.Attachments ?? entry.Attachments;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Обновлена запись в журнале: {entry.Id}");

            return Ok(new { message = "Запись успешно обновлена" });
        }

        /// <summary>
        /// Удалить запись (архивировать)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == id);

            if (entry == null)
                return NotFound(new { message = "Запись не найдена" });

            // Только автор может удалять
            if (entry.AuthorId != userId)
                return Forbid();

            entry.IsArchived = true;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Обновлена запись в журнале: {entry.Id}");

            return Ok(new { message = "Запись успешно удалена" });
        }

        /// <summary>
        /// Поиск записей по тегам или ключевым словам
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchEntries(
            [FromQuery] string query = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(query))
                return BadRequest(new { message = "Поисковый запрос не может быть пустым" });

            var searchResults = await _context.JournalEntries
                .Where(j => j.Experiment.ResearcherId == userId && !j.IsArchived &&
                    (j.Title.Contains(query) ||
                     j.Content.Contains(query) ||
                     j.Tags.Contains(query)))
                .OrderByDescending(j => j.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Content,
                    j.Type,
                    j.Tags,
                    j.CreatedAt,
                    Experiment = j.Experiment.Title,
                    Author = j.Author.FullName
                })
                .ToListAsync();

            return Ok(new { total = searchResults.Count, data = searchResults });
        }
    }

    // DTO классы
    public class CreateJournalEntryRequest
    {
        public int ExperimentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public EntryType Type { get; set; }
        public Priority? Priority { get; set; }
        public string Tags { get; set; } = string.Empty;
        public string Attachments { get; set; } = string.Empty;
    }

    public class UpdateJournalEntryRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public EntryType? Type { get; set; }
        public Priority? Priority { get; set; }
        public string? Tags { get; set; }
        public string? Attachments { get; set; }
    }
}
