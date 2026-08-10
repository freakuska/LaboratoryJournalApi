using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaboratoryJournal.Data;
using LaboratoryJournal.Models;

namespace LaboratoryJournal.Controllers
{
    /// <summary>
    /// Контроллер управления результатами экспериментов
    /// </summary>
    [ApiController]
    [Route("api/experiment-results")]
    [Authorize]
    public class ExperimentResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExperimentResultsController> _logger;

        public ExperimentResultsController(ApplicationDbContext context, ILogger<ExperimentResultsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить результаты для конкретного эксперимента
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetResults(
            [FromQuery] string searchTerm = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = _context.ExperimentResults
                .Where(r => r.Experiment.ResearcherId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.Name.Contains(searchTerm) ||
                    r.Description.Contains(searchTerm) ||
                    r.Value.Contains(searchTerm));
            }

            var total = await query.CountAsync();
            var results = await query
                .OrderByDescending(r => r.RecordedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.ExperimentId,
                    r.Name,
                    r.Description,
                    r.Value,
                    r.Unit,
                    r.DataType,
                    r.Status,
                    r.RecordedAt,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, pageNumber, pageSize, data = results });
        }

        [HttpGet("experiment/{experimentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetResultsByExperiment(
            int experimentId,
            [FromQuery] string searchTerm = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Проверка доступа
            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == experimentId && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            var query = _context.ExperimentResults
                .Where(r => r.ExperimentId == experimentId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.Name.Contains(searchTerm) ||
                    r.Description.Contains(searchTerm) ||
                    r.Value.Contains(searchTerm));
            }

            var total = await query.CountAsync();
            var results = await query
                .OrderByDescending(r => r.RecordedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.Value,
                    r.Unit,
                    r.DataType,
                    r.Status,
                    r.RecordedAt,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, pageNumber, pageSize, data = results });
        }

        /// <summary>
        /// Получить конкретный результат
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetResult(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _context.ExperimentResults
                .Include(r => r.Experiment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
                return NotFound(new { message = "Результат не найден" });

            if (result.Experiment.ResearcherId != userId)
                return Forbid();

            return Ok(new
            {
                result.Id,
                result.Name,
                result.Description,
                result.Value,
                result.Unit,
                result.DataType,
                result.Status,
                result.Notes,
                result.RecordedAt,
                result.CreatedAt,
                Experiment = result.Experiment.Title
            });
        }

        /// <summary>
        /// Создать новый результат
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateResult([FromBody] CreateExperimentResultRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Проверка, что эксперимент существует и принадлежит пользователю
            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == request.ExperimentId && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            var result = new ExperimentResult
            {
                ExperimentId = request.ExperimentId,
                Name = request.Name,
                Description = request.Description,
                Value = request.Value,
                Unit = request.Unit,
                DataType = request.DataType,
                Status = ResultStatus.Pending,
                Notes = request.Notes,
                RecordedAt = request.RecordedAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExperimentResults.Add(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Создан новый результат эксперимента: {result.Id}");

            return CreatedAtAction(nameof(GetResult), new { id = result.Id }, new
            {
                result.Id,
                result.Name,
                result.Value,
                result.Status,
                result.CreatedAt
            });
        }

        /// <summary>
        /// Обновить результат
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateExperimentResultRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _context.ExperimentResults
                .Include(r => r.Experiment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
                return NotFound(new { message = "Результат не найден" });

            if (result.Experiment.ResearcherId != userId)
                return Forbid();

            result.Name = request.Name ?? result.Name;
            result.Description = request.Description ?? result.Description;
            result.Value = request.Value ?? result.Value;
            result.Unit = request.Unit ?? result.Unit;
            result.Status = request.Status ?? result.Status;
            result.Notes = request.Notes ?? result.Notes;

            _context.ExperimentResults.Update(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Обновлен результат эксперимента: {result.Id}");

            return Ok(new { message = "Результат успешно обновлён" });
        }

        /// <summary>
        /// Удалить результат
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _context.ExperimentResults
                .Include(r => r.Experiment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
                return NotFound(new { message = "Результат не найден" });

            if (result.Experiment.ResearcherId != userId)
                return Forbid();

            _context.ExperimentResults.Remove(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Удален результат эксперимента: {result.Id}");

            return Ok(new { message = "Результат успешно удалён" });
        }

        /// <summary>
        /// Получить статистику по результатам
        /// </summary>
        [HttpGet("experiment/{experimentId}/statistics")]
        public async Task<ActionResult<object>> GetStatistics(int experimentId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var experiment = await _context.Experiments
                .FirstOrDefaultAsync(e => e.Id == experimentId && e.ResearcherId == userId);

            if (experiment == null)
                return NotFound(new { message = "Эксперимент не найден" });

            var results = await _context.ExperimentResults
                .Where(r => r.ExperimentId == experimentId)
                .ToListAsync();

            return Ok(new
            {
                TotalResults = results.Count,
                ByStatus = new
                {
                    Pending = results.Count(r => r.Status == ResultStatus.Pending),
                    Verified = results.Count(r => r.Status == ResultStatus.Verified),
                    Approved = results.Count(r => r.Status == ResultStatus.Approved),
                    Rejected = results.Count(r => r.Status == ResultStatus.Rejected),
                    Archived = results.Count(r => r.Status == ResultStatus.Archived)
                },
                ByDataType = new
                {
                    Numeric = results.Count(r => r.DataType == DataType.Numeric),
                    Text = results.Count(r => r.DataType == DataType.Text),
                    Boolean = results.Count(r => r.DataType == DataType.Boolean),
                    File = results.Count(r => r.DataType == DataType.File),
                    Image = results.Count(r => r.DataType == DataType.Image)
                }
            });
        }
    }

    // DTO классы
    public class CreateExperimentResultRequest
    {
        public int ExperimentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public DataType DataType { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? RecordedAt { get; set; }
    }

    public class UpdateExperimentResultRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Value { get; set; }
        public string? Unit { get; set; }
        public ResultStatus? Status { get; set; }
        public string? Notes { get; set; }
    }
}
