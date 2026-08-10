using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaboratoryJournal.Data;
using LaboratoryJournal.Models;

namespace LaboratoryJournal.Controllers
{
    /// <summary>
    /// Контроллер управления экспериментами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperimentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExperimentsController> _logger;

        public ExperimentsController(ApplicationDbContext context, ILogger<ExperimentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить список всех экспериментов пользователя
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetExperiments(
            [FromQuery] string searchTerm = "",
            [FromQuery] ExperimentStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = _context.Experiments
                .Where(e => e.ResearcherId == userId)
                .AsQueryable();

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.Title.Contains(searchTerm) || e.Description.Contains(searchTerm));
            }

            // Фильтрация по статусу
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            var total = await query.CountAsync();
            var experiments = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Objective,
                    e.Status,
                    e.StartDate,
                    e.EndDate,
                    e.CreatedAt,
                    e.UpdatedAt,
                    ResultsCount = e.Results.Count,
                    EntriesCount = e.JournalEntries.Count
                })
                .ToListAsync();

            return Ok(new { total, pageNumber, pageSize, data = experiments });
        }

        /// <summary>
        /// Получить информацию о конкретном эксперименте
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetExperiment(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var experiment = await _context.Experiments
                .Where(e => e.Id == id && e.ResearcherId == userId)
                .Include(e => e.Results)
                .Include(e => e.JournalEntries)
                .FirstOrDefaultAsync();

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            return Ok(new
            {
                experiment.Id,
                experiment.Title,
                experiment.Description,
                experiment.Objective,
                experiment.Methodology,
                experiment.Status,
                experiment.StartDate,
                experiment.EndDate,
                experiment.CreatedAt,
                experiment.UpdatedAt,
                Results = experiment.Results.Count,
                Entries = experiment.JournalEntries.Count
            });
        }

        /// <summary>
        /// Создать новый эксперимент
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateExperiment([FromBody] CreateExperimentRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var experiment = new Experiment
            {
                Title = request.Title,
                Description = request.Description,
                Objective = request.Objective,
                Methodology = request.Methodology,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = ExperimentStatus.Planning,
                ResearcherId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Experiments.Add(experiment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Создан новый эксперимент: {experiment.Id}");

            return CreatedAtAction(nameof(GetExperiment), new { id = experiment.Id }, new
            {
                experiment.Id,
                experiment.Title,
                experiment.Status,
                experiment.CreatedAt
            });
        }

        /// <summary>
        /// Обновить эксперимент
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExperiment(int id, [FromBody] UpdateExperimentRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == id && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            experiment.Title = request.Title ?? experiment.Title;
            experiment.Description = request.Description ?? experiment.Description;
            experiment.Objective = request.Objective ?? experiment.Objective;
            experiment.Methodology = request.Methodology ?? experiment.Methodology;
            experiment.Status = request.Status ?? experiment.Status;
            experiment.EndDate = request.EndDate ?? experiment.EndDate;
            experiment.UpdatedAt = DateTime.UtcNow;

            _context.Experiments.Update(experiment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Обновлен эксперимент: {experiment.Id}");

            return Ok(new { message = "Эксперимент успешно обновлён" });
        }

        /// <summary>
        /// Удалить эксперимент
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExperiment(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == id && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            _context.Experiments.Remove(experiment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Удален эксперимент: {experiment.Id}");

            return Ok(new { message = "Эксперимент успешно удалён" });
        }
    }

    // DTO классы
    public class CreateExperimentRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Objective { get; set; }
        public string Methodology { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateExperimentRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Objective { get; set; }
        public string Methodology { get; set; }
        public ExperimentStatus? Status { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
