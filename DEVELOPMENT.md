# DEVELOPMENT.md - Руководство по развитию проекта

## 📊 Статус проекта

**Версия**: 1.0.0 MVP (Minimum Viable Product)  
**Статус**: ✅ Базовая функциональность завершена  
**Дата**: 15 апреля 2026

## ✅ Завершённые функции (Фаза 1)

### Core API Endpoints
- [x] Система аутентификации (регистрация, вход, выход)
- [x] Управление профилем пользователя
- [x] CRUD операции для экспериментов
- [x] CRUD операции для записей журнала
- [x] CRUD операции для результатов
- [x] Глобальный поиск
- [x] Расширенный поиск с фильтрами
- [x] Система ролей (Admin, Researcher, Moderator)

### Infrastructure
- [x] Entity Framework Core 8.0 с SQL Server
- [x] ASP.NET Core Identity для управления пользователями
- [x] Миграции базы данных
- [x] Swagger/OpenAPI документация
- [x] CORS политика
- [x] Логирование

## 🚧 Заплани на разработку (Фаза 2-5)

### ФАЗА 2: Расширенные функции (1-2 месяца)

#### 2.1 Система комментариев и обсуждений
```csharp
// Файл: Models/Comment.cs
public class Comment
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public string AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    
    public virtual JournalEntry JournalEntry { get; set; }
    public virtual ApplicationUser Author { get; set; }
}
```

**Приоритет**: ВЫСОКИЙ  
**Сложность**: СРЕДНЯЯ  
**Endpoint'ы**:
- `POST /api/comments` - Создать комментарий
- `GET /api/comments/entry/{entryId}` - Получить комментарии записи
- `PUT /api/comments/{id}` - Редактировать комментарий
- `DELETE /api/comments/{id}` - Удалить комментарий

#### 2.2 Загрузка файлов
```csharp
// Файл: Models/FileAttachment.cs
public class FileAttachment
{
    public int Id { get; set; }
    public int ExperimentResultId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
    
    public virtual ExperimentResult ExperimentResult { get; set; }
}
```

**Приоритет**: ВЫСОКИЙ  
**Сложность**: СРЕДНЯЯ  
**Реализация**:
- Сохранение в локальной папке `/uploads` или Azure Blob Storage
- Ограничение размера файла (max 100MB)
- Типы: PDF, PNG, JPG, XLSX, CSV

#### 2.3 Уведомления
```csharp
// Файл: Models/Notification.cs
public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual ApplicationUser User { get; set; }
}

public enum NotificationType
{
    ExperimentCreated,
    ResultAdded,
    CommentAdded,
    ExperimentCompleted
}
```

### ФАЗА 3: Web интерфейс (2-3 месяца)

#### 3.1 Frontend на React
```bash
# Новый проект
npx create-react-app LaboratoryJournal.UI
cd LaboratoryJournal.UI

# Основные Library
npm install axios react-router-dom material-ui recharts
npm install @redux/toolkit react-redux

# Структура
src/
├── components/
│   ├── Auth/
│   ├── Experiments/
│   ├── JournalEntries/
│   └── Results/
├── services/
│   └── api.ts
├── store/
│   └── index.ts
└── pages/
```

**Key Components**:
- Authentication pages (Login, Register)
- Experiments dashboard
- Journal entries editor
- Results visualization
- Search interface

### ФАЗА 4: Advanced Features (3-4 месяца)

#### 4.1 Экспорт в PDF/Excel
```csharp
// Использовать: iTextSharp для PDF, EPPlus для Excel
public class ExportService
{
    public byte[] ExportToPdf(int experimentId);
    public byte[] ExportToExcel(List<ExperimentResult> results);
}
```

#### 4.2 Аналитика и статистика
```csharp
// Controllers/AnalyticsController.cs
[HttpGet("experiment/{experimentId}/analytics")]
public async Task<ActionResult> GetExperimentAnalytics(int experimentId)
{
    // Статистика:
    // - Количество записей по дням
    // - Распределение типов результатов
    // - Активность пользователей
    // - Графики результатов
}
```

### ФАЗА 5: Integration & Optimization (4-5 месяцев)

#### 5.1 REST API оптимизация
- Кэширование запросов (Redis)
- Асинхронная обработка (Background Jobs)
- Rate limiting

#### 5.2 Тестирование
```bash
# Unit тесты
dotnet new xunit -o LaboratoryJournal.Tests
dotnet add LaboratoryJournal.Tests/LaboratoryJournal.Tests.csproj reference LaboratoryJournal/LaboratoryJournal.csproj

# xUnit тесты
[Fact]
public async Task CreateExperiment_WithValidData_ReturnsSuccess()
{
    // Test code
}
```

#### 5.3 CI/CD Pipeline (GitHub Actions)
```yaml
# .github/workflows/build.yml
name: Build and Test
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/setup-dotnet@v1
    - run: dotnet build
    - run: dotnet test
```

## 🔧 Процесс разработки новых функций

### Шаблон создания новой функции:

1. **Создание Model**:
```csharp
// Models/NewFeature.cs
public class NewFeature
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

2. **Обновление DbContext**:
```csharp
// Data/ApplicationDbContext.cs
public DbSet<NewFeature> NewFeatures { get; set; }

// OnModelCreating
modelBuilder.Entity<NewFeature>()
    .HasIndex(n => n.Name);
```

3. **Создание Миграции**:
```bash
dotnet ef migrations add Add_NewFeature_Table
dotnet ef database update
```

4. **Создание DTO**:
```csharp
public class CreateNewFeatureRequest
{
    [Required]
    public string Name { get; set; }
}
```

5. **Реализация Service**:
```csharp
// Services/NewFeatureService.cs
public interface INewFeatureService
{
    Task<NewFeature> CreateAsync(CreateNewFeatureRequest request);
    Task<NewFeature> GetByIdAsync(int id);
}
```

6. **Создание Controller**:
```csharp
// Controllers/NewFeaturesController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NewFeaturesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateNewFeatureRequest request)
    {
        // Implementation
    }
}
```

7. **Тестирование**:
```csharp
[Fact]
public async Task CreateNewFeature_ValidRequest_ReturnsOk()
{
    // Test
}
```

## 📚 Используемые библиотеки для расширения

### Для PDF генерации:
```bash
dotnet add package iTextSharp.LGPLv2.Core
# или
dotnet add package SelectPdf
```

### Для Excel:
```bash
dotnet add package EPPlus
```

### Для Email:
```bash
dotnet add package MailKit
```

### Для Background Jobs:
```bash
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer
```

### Для Кэширования:
```bash
dotnet add package StackExchange.Redis
```

### Для Логирования:
```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
```

## 🐛 Известные проблемы и улучшения

1. **Performance**
   - Добавить пагинацию ко всем list endpoints
   - Реализовать кэширование результатов поиска
   - Оптимизировать запросы к БД (N+1 проблема)

2. **Security**
   - Добавить двухфакторную аутентификацию
   - Внедрить JWT токены вместо session
   - Добавить Rate limiting на API

3. **Usability**
   - Улучшить обработку ошибок (более подробные сообщения)
   - Добавить валидацию на фронтенде
   - Реализовать WebSocket для real-time обновлений

## 📋 Чеклист для каждого релиза

```markdown
- [ ] Все тесты проходят
- [ ] Code review выполнен
- [ ] Документация обновлена
- [ ] Нет breaking changes
- [ ] Миграции протестированы
- [ ] Performance не деградировал
- [ ] Security issues решены
- [ ] Релизные заметки написаны
```

## 🚀 Развёртывание

### Lokальное тестирование:
```bash
dotnet run
```

### Docker контейнеризация:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build "LaboratoryJournal/LaboratoryJournal.csproj" -c Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /src/bin/Release/net9.0 .
ENTRYPOINT ["dotnet", "LaboratoryJournal.dll"]
```

### Публикация:
```bash
dotnet publish -c Release -o ./publish
```

## 📞 Contact & Support

Для обсуждения архитектуры или вопросов разработки, создавайте Issues в репозитории.

---

**Последнее обновление**: 15 апреля 2026  
**Версия**: 1.0.0
