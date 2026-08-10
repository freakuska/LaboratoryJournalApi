# ARCHITECTURE.md - Архитектура приложения

## 🏗️ Общая архитектура

Приложение Laboratory Journal построено на архитектуре **Layered Architecture** (многоуровневая архитектура) с использованием паттернов **Repository Pattern** и **Dependency Injection**.

```
┌─────────────────────────────────────────────────────────┐
│                    API Controllers                       │
│  (AuthController, ExperimentsController, etc.)          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│            Business Logic / Services Layer              │
│  (будет расширено на этапе разработки)                 │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│            Data Access Layer (EF Core)                  │
│  ApplicationDbContext, DbSet<T>                         │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              SQL Server Database                         │
│  LaboratoryJournalDb                                    │
└─────────────────────────────────────────────────────────┘
```

## 📁 Структура проекта

```
LaboratoryJournal/
├── Controllers/
│   ├── AuthController.cs              # Аутентификация
│   ├── ExperimentsController.cs        # Управление экспериментами
│   ├── JournalEntriesController.cs     # Записи журнала
│   ├── ExperimentResultsController.cs  # Результаты
│   └── SearchController.cs             # Поиск
│
├── Models/                             # Домен модели
│   ├── ApplicationUser.cs              # Пользователь (наследуется от IdentityUser)
│   ├── Experiment.cs                   # Эксперимент
│   ├── ExperimentResult.cs             # Результат эксперимента
│   └── JournalEntry.cs                 # Запись журнала
│
├── Data/
│   └── ApplicationDbContext.cs         # EF Core DbContext
│
├── Services/                           # Бизнес-логика (будет расширено)
│
├── Migrations/
│   ├── 20260415000000_InitialCreate.cs
│   └── ...
│
├── Program.cs                          # Entry point, конфигурация DI
├── appsettings.json                    # Конфигурация
├── appsettings.Development.json        # Development конфигурация
├── LaboratoryJournal.csproj           # Файл проекта
├── .gitignore
├── README.md
├── DEVELOPMENT.md
├── INSTALLATION_GUIDE.md
├── API_USAGE_GUIDE.md
└── ARCHITECTURE.md (этот файл)
```

## 🔄 DbContext и Entity Relationships

### ApplicationDbContext

Наследуется от `IdentityDbContext<ApplicationUser>` для встроенной поддержки Asp.NET Identity.

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Experiment> Experiments { get; set; }
    public DbSet<ExperimentResult> ExperimentResults { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
}
```

### Entity Relationships (связи)

```
ApplicationUser
├── 1 ---→ N : Experiments
└── 1 ---→ N : JournalEntries

Experiment
├── 1 ---→ N : ExperimentResults
├── 1 ---→ N : JournalEntries
└── N ---← 1 : ApplicationUser

ExperimentResult
└── N ---← 1 : Experiment

JournalEntry
├── N ---← 1 : Experiment
└── N ---← 1 : ApplicationUser
```

## 🔐 Слой аутентификации и авторизации

### ASP.NET Core Identity

Использует встроенный механизм идентификации:
- UserManager<ApplicationUser> - управление пользователями
- SignInManager<ApplicationUser> - вход/выход
- RoleManager<IdentityRole> - управление ролями

### Роли системы

1. **Admin** - полный доступ ко всем функциям
2. **Researcher** - основная роль, может создавать эксперименты
3. **Moderator** - надзирает за качеством данных

### Authorization

Все endpoints (кроме `/auth/register` и `/auth/login`) защищены атрибутом `[Authorize]`.

## 🗂️ API структура

### Endpoints grouping

```
/api/auth/                  # Authentication
/api/experiments/           # Experiments management
/api/journalentries/        # Journal entries
/api/experimentresults/     # Results
/api/search/                # Search functionality
```

### DTO Pattern

Для каждого endpoint используются DTO классы для Request/Response:

```csharp
// Request DTOs
public class CreateExperimentRequest { ... }
public class UpdateExperimentRequest { ... }

// Response - выполняется через анонимные типы (object)
return Ok(new { id, title, status });
```

## 📊 Модели и их свойства

### ApplicationUser

```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}
```

### Experiment

```csharp
public class Experiment
{
    public int Id { get; set; }
    public string Title { get; set; }                  // Max 255
    public string Description { get; set; }            // Max 2000
    public string Objective { get; set; }
    public string Methodology { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ExperimentStatus Status { get; set; }       // Enum
    public string ResearcherId { get; set; }           // FK
    public DateTime CreatedAt { get; set; } = UtcNow
    public DateTime UpdatedAt { get; set; } = UtcNow
    
    // Navigation properties
    public ApplicationUser Researcher { get; set; }
    public ICollection<ExperimentResult> Results { get; set; }
    public ICollection<JournalEntry> JournalEntries { get; set; }
}
```

### ExperimentResult

```csharp
public class ExperimentResult
{
    public int Id { get; set; }
    public int ExperimentId { get; set; }              // FK
    public string Name { get; set; }                    // Max 255
    public string Description { get; set; }
    public string Value { get; set; }                   // Max 1000
    public string Unit { get; set; }
    public DataType DataType { get; set; }             // Enum
    public ResultStatus Status { get; set; }           // Enum
    public string Notes { get; set; }
    public DateTime RecordedAt { get; set; } = UtcNow
    public DateTime CreatedAt { get; set; } = UtcNow
    
    // Navigation property
    public Experiment Experiment { get; set; }
}
```

### JournalEntry

```csharp
public class JournalEntry
{
    public int Id { get; set; }
    public int ExperimentId { get; set; }              // FK
    public string AuthorId { get; set; }               // FK
    public string Title { get; set; }                   // Max 255
    public string Content { get; set; }                 // Max 5000
    public EntryType Type { get; set; }                // Enum
    public Priority Priority { get; set; }             // Enum
    public string Tags { get; set; }
    public string Attachments { get; set; }
    public DateTime CreatedAt { get; set; } = UtcNow
    public DateTime UpdatedAt { get; set; } = UtcNow
    public bool IsArchived { get; set; } = false
    
    // Navigation properties
    public Experiment Experiment { get; set; }
    public ApplicationUser Author { get; set; }
}
```

## 🔍 Индексы базы данных

Созданы для оптимизации поиска:

```sql
-- Experiments
CREATE INDEX IX_Experiment_Title ON Experiments(Title)
CREATE INDEX IX_Experiment_ResearcherId ON Experiments(ResearcherId)
CREATE INDEX IX_Experiment_StartDate ON Experiments(StartDate)

-- JournalEntries
CREATE INDEX IX_JournalEntry_ExperimentId ON JournalEntries(ExperimentId)
CREATE INDEX IX_JournalEntry_AuthorId ON JournalEntries(AuthorId)
CREATE INDEX IX_JournalEntry_Title ON JournalEntries(Title)
CREATE INDEX IX_JournalEntry_Tags ON JournalEntries(Tags)
```

## 🔐 Безопасность и validация

### Model Validation

Используется DataAnnotations:
```csharp
[Required(ErrorMessage = "Email обязателен")]
[EmailAddress]
public string Email { get; set; }

[MinLength(8)]
[Required]
public string Password { get; set; }
```

### Cascading Delete

Настроено для автоматического удаления связанных данных:
```csharp
.HasMany(u => u.Experiments)
.WithOne(e => e.Researcher)
.HasForeignKey(e => e.ResearcherId)
.OnDelete(DeleteBehavior.Cascade);
```

### Data Access Control

Каждый пользователь видит только свои данные:
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var experiments = _context.Experiments
    .Where(e => e.ResearcherId == userId);
```

## 📝 Логирование

Использует встроенный `ILogger`:
```csharp
private readonly ILogger<ExperimentsController> _logger;

_logger.LogInformation($"Создан новый эксперимент: {experiment.Id}");
_logger.LogWarning($"Попытка входа с несуществующим адресом: {email}");
_logger.LogError(ex, "Произошла ошибка при обновлении");
```

## 🧪 Тестирование (планируется)

### Unit Testing

```csharp
[Fact]
public async Task CreateExperiment_WithValidData_ReturnsOk()
{
    // Arrange
    var controller = new ExperimentsController(_context, _logger);
    var request = new CreateExperimentRequest { ... };
    
    // Act
    var result = await controller.CreateExperiment(request);
    
    // Assert
    Assert.Equal(201, result.StatusCode);
}
```

### Integration Testing

Использование TestContainers для real БД:
```csharp
public class IntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    
    public async Task InitializeAsync() => await _container.StartAsync();
}
```

## 🚀 Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LaboratoryJournal/LaboratoryJournal.csproj", "LaboratoryJournal/"]
RUN dotnet restore "LaboratoryJournal/LaboratoryJournal.csproj"
COPY . .
RUN dotnet build "LaboratoryJournal/LaboratoryJournal.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "LaboratoryJournal.dll"]
```

### Azure Deployment

1. Создать App Service на Azure
2. Создать SQL Database на Azure
3. Обновить connection string
4. Deploy через Azure DevOps или GitHub Actions

## 🔄 CI/CD Pipeline

### GitHub Actions

```yaml
name: Build and Deploy
on: [push]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '9.0'
    - run: dotnet build
    - run: dotnet test
```

## 📚 Паттерны и принципы

### Используемые паттерны

1. **Repository Pattern** - (будет добавлено) для абстракции доступа к данным
2. **Dependency Injection** - все сервисы регистрируются в `Program.cs`
3. **DTO Pattern** - для передачи данных между слоями
4. **Factory Pattern** - создание объектов Identity roles
5. **Observer Pattern** - логирование через `ILogger`

### SOLID принципы

- **S**ingle Responsibility - каждый controller отвечает за один ресурс
- **O**pen/Closed - легко расширять через наследование
- **L**iskov Substitution - использование interfaces для abstraction
- **I**nterface Segregation - малые, специализированные interfaces
- **D**ependency Inversion - зависимости от abstractions, не от implementations

## 🎯 Performance Considerations

### Оптимизации

1. **Пагинация**: все list endpoints поддерживают `pageNumber` и `pageSize`
2. **Индексы БД**: созданы на часто используемых columns
3. **Async/Await**: все database операции асинхронные
4. **Select optimization**: использование `Select()` для проекций вместо загрузки всех полей

### Будущие оптимизации

1. **Кэширование**: Redis для часто используемых запросов
2. **Connection pooling**: оптимизация EF Core
3. **Database normalization**: дальнейшая оптимизация схемы
4. **Query optimization**: EXPLAIN PLAN analysis

## 🔄 Миграции версионирования

### EF Core Migrations

```bash
# Создание миграции
dotnet ef migrations add MigrationName

# Применение
dotnet ef database update

# Откат
dotnet ef database update PreviousMigrationName
```

---

**Документ версии**: 1.0  
**Дата обновления**: 15 апреля 2026  
**Совместимость**: .NET 9.0, C# 12
