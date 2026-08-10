using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaboratoryJournal.Data;
using LaboratoryJournal.Models;

namespace LaboratoryJournal.Controllers
{
    /// <summary>
    /// Контроллер для поиска по всем данным
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ApplicationDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Глобальный поиск по всем данным пользователя
        /// </summary>
        [HttpGet("global")]
        public async Task<ActionResult<object>> GlobalSearch(
            [FromQuery] string query = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(query) || query.Length < 2)
                return BadRequest(new { message = "Поисковый запрос должен содержать минимум 2 символа" });

            var searchQuery = query.ToLower();
            var skip = (pageNumber - 1) * pageSize;

            // Поиск в экспериментах
            var experiments = await _context.Experiments
                .Where(e => e.ResearcherId == userId &&
                    (e.Title.ToLower().Contains(searchQuery) ||
                     e.Description.ToLower().Contains(searchQuery) ||
                     e.Objective.ToLower().Contains(searchQuery)))
                .Select(e => new
                {
                    Type = "Experiment",
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    Score = e.Title.ToLower().Contains(searchQuery) ? 100 : 50
                })
                .ToListAsync();

            // Поиск в записях журнала
            var entries = await _context.JournalEntries
                .Where(j => j.Experiment.ResearcherId == userId && !j.IsArchived &&
                    (j.Title.ToLower().Contains(searchQuery) ||
                     j.Content.ToLower().Contains(searchQuery) ||
                     j.Tags.ToLower().Contains(searchQuery)))
                .Select(j => new
                {
                    Type = "JournalEntry",
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Content,
                    CreatedAt = j.CreatedAt,
                    Score = j.Title.ToLower().Contains(searchQuery) ? 100 : 
                            j.Tags.ToLower().Contains(searchQuery) ? 75 : 50
                })
                .ToListAsync();

            // Поиск в результатах
            var results = await _context.ExperimentResults
                .Where(r => r.Experiment.ResearcherId == userId &&
                    (r.Name.ToLower().Contains(searchQuery) ||
                     r.Description.ToLower().Contains(searchQuery) ||
                     r.Value.ToLower().Contains(searchQuery)))
                .Select(r => new
                {
                    Type = "Result",
                    Id = r.Id,
                    Title = r.Name,
                    Description = r.Value,
                    CreatedAt = r.RecordedAt,
                    Score = r.Name.ToLower().Contains(searchQuery) ? 100 : 50
                })
                .ToListAsync();

            // Объединение и сортировка по скору и дате
            var combinedResults = experiments
                .Concat(entries)
                .Concat(results)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation($"Выполнен глобальный поиск: '{query}' найдено результатов: {combinedResults.Count}");

            return Ok(new
            {
                Query = query,
                Total = experiments.Count + entries.Count + results.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = combinedResults
            });
        }

        /// <summary>
        /// Расширенный поиск по экспериментам
        /// </summary>
        [HttpPost("experiments")]
        public async Task<ActionResult<IEnumerable<object>>> SearchExperiments([FromBody] AdvancedSearchRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = _context.Experiments
                .Where(e => e.ResearcherId == userId)
                .AsQueryable();

            // По названию
            if (!string.IsNullOrEmpty(request.Title))
            {
                query = query.Where(e => e.Title.Contains(request.Title));
            }

            // По описанию
            if (!string.IsNullOrEmpty(request.Description))
            {
                query = query.Where(e => e.Description.Contains(request.Description));
            }

            // По статусу
            if (request.Status.HasValue)
            {
                query = query.Where(e => e.Status == request.Status.Value);
            }

            // По диапазону дат
            if (request.StartDateFrom.HasValue)
            {
                query = query.Where(e => e.StartDate >= request.StartDateFrom.Value);
            }

            if (request.StartDateTo.HasValue)
            {
                query = query.Where(e => e.StartDate <= request.StartDateTo.Value);
            }

            var experiments = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Objective,
                    e.Status,
                    e.StartDate,
                    e.EndDate,
                    e.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation($"Выполнен расширенный поиск экспериментов, найдено: {experiments.Count}");

            return Ok(experiments);
        }

        /// <summary>
        /// Поиск по тегам
        /// </summary>
        [HttpGet("tags")]
        public async Task<ActionResult<IEnumerable<object>>> SearchByTags([FromQuery] string tags = "")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(tags))
                return BadRequest(new { message = "Теги не указаны" });

            var tagList = tags.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .ToList();

            var entries = await _context.JournalEntries
                .Where(j => j.Experiment.ResearcherId == userId && !j.IsArchived)
                .ToListAsync();

            var filteredEntries = entries
                .Where(j => tagList.Any(tag => j.Tags?.ToLower().Contains(tag) ?? false))
                .GroupBy(j => j.ExperimentId)
                .Select(g => new
                {
                    ExperimentId = g.Key,
                    Entries = g.Select(j => new
                    {
                        j.Id,
                        j.Title,
                        j.Tags,
                        j.CreatedAt
                    }).ToList()
                })
                .ToList();

            return Ok(filteredEntries);
        }

        /// <summary>
        /// Получить рекомендации на основе истории поиска
        /// </summary>
        [HttpGet("recommendations")]
        public async Task<ActionResult<object>> GetRecommendations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Получить недавно обновлённые эксперименты
            var recentExperiments = await _context.Experiments
                .Where(e => e.ResearcherId == userId)
                .OrderByDescending(e => e.UpdatedAt)
                .Take(5)
                .Select(e => new
                {
                    Type = "Experiment",
                    e.Id,
                    e.Title,
                    e.UpdatedAt
                })
                .ToListAsync();

            // Получить активные эксперименты
            var activeExperiments = await _context.Experiments
                .Where(e => e.ResearcherId == userId && e.Status == ExperimentStatus.InProgress)
                .Select(e => new
                {
                    Type = "Active",
                    e.Id,
                    e.Title,
                    e.StartDate
                })
                .ToListAsync();

            // Получить последние записи журнала
            var recentEntries = await _context.JournalEntries
                .Where(j => j.AuthorId == userId && !j.IsArchived)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(e => new
                {
                    Type = "JournalEntry",
                    e.Id,
                    e.Title,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                RecentExperiments = recentExperiments,
                ActiveExperiments = activeExperiments,
                RecentEntries = recentEntries
            });
        }
    }

    // DTO классы
    public class AdvancedSearchRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ExperimentStatus? Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
    }
}
