# Laboratory Journal API

REST API для ведения лабораторных экспериментов, записей исследователей и результатов измерений. Проект демонстрирует разработку серверного приложения на ASP.NET Core с JWT-аутентификацией, разграничением данных пользователей и автоматическими тестами.

## Возможности

- регистрация и аутентификация пользователей;
- JWT-аутентификация и роли `Admin`, `Researcher`, `Moderator`;
- создание и управление экспериментами;
- хранение результатов измерений и их статусов;
- ведение записей лабораторного журнала;
- поиск по экспериментам, записям, результатам и тегам;
- пагинация и фильтрация данных;
- Swagger/OpenAPI в среде разработки;
- проверка состояния приложения через `GET /health`.

## Стек

- C# и .NET 8;
- ASP.NET Core Web API;
- Entity Framework Core;
- SQL Server / LocalDB;
- ASP.NET Core Identity;
- JWT Bearer Authentication;
- Swagger/OpenAPI;
- xUnit и GitHub Actions.

## Архитектура

```text
Controllers/       HTTP API и проверка доступа
Data/              EF Core DbContext
Models/            доменные модели
Options/           типизированная конфигурация
Services/          прикладные сервисы
tests/             автоматические тесты
wwwroot/           демонстрационный веб-интерфейс
```

Каждый запрос к экспериментам, результатам и записям ограничивается данными текущего пользователя. JWT-секрет не хранится в репозитории и передаётся через переменную окружения или `dotnet user-secrets`.

## Запуск

### Требования

- .NET 8 SDK;
- SQL Server LocalDB или доступный экземпляр SQL Server;
- EF Core CLI для создания и применения миграций.

### 1. Клонирование

```bash
git clone https://github.com/freakuska/LaboratoryJournalApi.git
cd LaboratoryJournalApi
```

### 2. Настройка JWT-секрета

Рекомендуемый вариант для локальной разработки:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "replace-with-a-random-secret-at-least-32-characters-long"
```

Либо используйте переменную окружения:

```bash
Jwt__Secret=replace-with-a-random-secret-at-least-32-characters-long
```

В PowerShell:

```powershell
$env:Jwt__Secret="replace-with-a-random-secret-at-least-32-characters-long"
```

### 3. Подготовка базы данных

Если начальная миграция ещё не создана:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Для автоматического применения существующих миграций при запуске установите `Database:ApplyMigrationsAtStartup` в `true`. В производственной среде миграции рекомендуется выполнять отдельным этапом развёртывания.

### 4. Запуск приложения

```bash
dotnet restore
dotnet run
```

Адреса локального запуска указаны в `Properties/launchSettings.json`. В режиме Development документация Swagger доступна по пути `/swagger`.

## Тестирование

```bash
dotnet test tests/LaboratoryJournal.Tests/LaboratoryJournal.Tests.csproj
```

При каждом push и pull request GitHub Actions автоматически выполняет восстановление зависимостей, сборку и тесты.

## Основные маршруты

| Метод | Маршрут | Назначение |
|---|---|---|
| `POST` | `/api/auth/register` | регистрация |
| `POST` | `/api/auth/login` | получение JWT |
| `GET` | `/api/auth/me` | текущий пользователь |
| `GET/POST` | `/api/experiments` | список и создание экспериментов |
| `GET/PUT/DELETE` | `/api/experiments/{id}` | управление экспериментом |
| `GET/POST` | `/api/experiment-results` | результаты измерений |
| `GET/POST` | `/api/journal-entries` | записи журнала |
| `GET` | `/api/search/global` | глобальный поиск |
| `GET` | `/health` | состояние API |

Защищённые маршруты требуют заголовок:

```http
Authorization: Bearer <token>
```

## Дополнительная документация

- [Архитектура](ARCHITECTURE.md)
- [Использование API](API_USAGE_GUIDE.md)

## Автор

Анна Енгалычева — разработка API, модели данных, аутентификация, бизнес-логика и тестирование.
