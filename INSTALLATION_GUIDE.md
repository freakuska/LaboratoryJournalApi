# Installation Guide - Руководство по установке

## 🖥️ Системные требования

- **OS**: Windows 10/11, macOS, или Linux
- **.NET**: 9.0 SDK или выше
- **SQL Server**: LocalDB, SQL Server Express, или SQL Server 2019+
- **RAM**: минимум 2GB
- **Disk Space**: минимум 500MB

## 📥 Шаг 1: Предварительная установка

### Windows

#### 1.1 Установка .NET 9.0 SDK
```bash
# Скачайте установщик с https://dotnet.microsoft.com/download
# или используйте chocolatey:
choco install dotnet-sdk
```

Проверка установки:
```bash
dotnet --version
```

#### 1.2 Установка SQL Server LocalDB
LocalDB обычно идёт с Visual Studio, но можно установить отдельно:

```bash
# Скачайте с https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
# Или через chocolatey:
choco install sql-server-localdb
```

Проверка:
```bash
sqllocaldb info mssqllocaldb
```

#### 1.3 Установка Git
```bash
# Скачайте с https://git-scm.com/download/win
# Или через chocolatey:
choco install git
```

Проверка:
```bash
git --version
```

### macOS

```bash
# Установка Homebrew (если ещё не установлен)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Установка .NET SDK
brew install dotnet-sdk

# Установка Git
brew install git
```

### Linux (Ubuntu/Debian)

```bash
# Обновление пакетов
sudo apt update

# Установка .NET SDK
sudo apt install dotnet-sdk-9.0

# Установка Git
sudo apt install git

# Для SQL Server на Linux (опционально):
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourPassword123' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server
```

## 🚀 Шаг 2: Клонирование репозитория

```bash
# Перейдите в желаемую директорию
cd C:\Projects  # на Windows
# или
cd ~/Projects   # на macOS/Linux

# Клонируйте репозиторий
git clone https://github.com/freakuska/Course_for_Yan.git
cd LaboratoryJournal
```

## ⚙️ Шаг 3: Восстановление зависимостей

```bash
dotnet restore
```

Это загрузит все NuGet пакеты, указанные в `LaboratoryJournal.csproj`:
- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
- Swashbuckle.AspNetCore 6.0.0
- и другие...

## 🗄️ Шаг 4: Конфигурация базы данных

### 4.1 Проверка строки подключения

Откройте файл `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LaboratoryJournalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 4.2 Применение миграций

```bash
dotnet ef database update
```

Это создаст базу данных и все необходимые таблицы.

Если вы хотите видеть SQL что будет выполняться:
```bash
dotnet ef database update --verbose
```

### 4.3 Проверка создания БД

Откройте SQL Server Management Studio или используйте команду:
```bash
sqlcmd -S (localdb)\mssqllocaldb -E
> SELECT name FROM sys.databases;
```

Должна появиться база `LaboratoryJournalDb`.

## 🔧 Шаг 5: Сборка приложения

```bash
dotnet build
```

Убедитесь что нет ошибок. Если есть ошибки, проверьте:
- Установлена ли .NET 9.0
- Восстановлены ли зависимости (`dotnet restore`)
- Правильная ли версия SQL Server

## ▶️ Шаг 6: Запуск приложения

```bash
dotnet run
```

или через watch для немедленной перезагрузки при изменениях:
```bash
dotnet watch run
```

После запуска вы должны увидеть:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop, restarting.
```

## 🌐 Шаг 7: Проверка приложения

### 7.1 Swagger UI документация
Откройте браузер и перейдите на:
```
https://localhost:5001/swagger/index.html
```

Должны увидеть интерактивную документацию API.

### 7.2 Проверка Health Check
```bash
curl -k https://localhost:5001/scripts/dbinit
# или просто откройте в браузере при доступе будет перенаправление
```

### 7.3 Тестирование API

Регистрация:
```bash
curl -k -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234567",
    "confirmPassword": "Test@1234567",
    "fullName": "Test User",
    "position": "Researcher"
  }'
```

## 🛠️ Смежные инструменты

### SQL Server Management Studio (SSMS)

Для управления БД:
1. Скачайте SSMS с https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
2. Подключитесь к `(localdb)\mssqllocaldb`
3. Просмотрите БД `LaboratoryJournalDb`

### Visual Studio Code

Расширения для удобства:
1. C# (powered by OmniSharp)
2. Entity Framework Core Power Tools
3. REST Client (для тестирования API)

### Postman

Для тестирования API:
1. Скачайте Postman с https://www.postman.com/download
2. Импортируйте Swagger JSON: `https://localhost:5001/swagger/v1/swagger.json`
3. Начните тестировать endpoints

## 🐛 Решение проблем

### Проблема 1: "Connection string 'DefaultConnection' not found"
**Решение**: Проверьте что `appsettings.json` находится в корне проекта и содержит ConnectionString.

### Проблема 2: SQL Server не запускается
```bash
# Проверьте статус LocalDB
sqllocaldb info

# Если не работает, переустановите
sqllocaldb stop mssqllocaldb
sqllocaldb delete mssqllocaldb
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb
```

### Проблема 3: "dotnet: command not found"
Убедитесь что .NET SDK установлен и добавлен в PATH:
```bash
# На Windows
setx PATH "%PATH%;C:\Program Files\dotnet"

# На macOS/Linux
export PATH="$PATH:/usr/local/share/dotnet"
```

### Проблема 4: Port 5001 занят
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :5001
kill -9 <PID>
```

### Проблема 5: CORS ошибки
Если фронтенд на другом порту, убедитесь что CORS настроен в `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

## 📦 Дополнительные команды полезные

### Очистка проекта
```bash
dotnet clean
```

### Восстановление пакетов с новыми версиями
```bash
dotnet package restore --force-evaluate
```

### Обновление NuGet пакетов
```bash
dotnet package upgrade
```

### Проверка версий установленных пакетов
```bash
dotnet package list
```

## 🔒 Конфигурация для Production

### Изменение appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server.database.windows.net;Database=LaboratoryJournal;User Id=admin;Password=YourPassword123;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Error"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

### Опубликование приложения

```bash
# Release build
dotnet publish -c Release -o ./publish

# Запуск из published версии
dotnet ./publish/LaboratoryJournal.dll
```

## ✅ Проверочный список After установки

- [ ] .NET 9.0 SDK установлен
- [ ] SQL Server LocalDB работает
- [ ] Репозиторий клонирован
- [ ] Зависимости восстановлены (`dotnet restore`)
- [ ] БД создана (`dotnet ef database update`)
- [ ] Приложение собирается (`dotnet build`)
- [ ] Приложение запускается (`dotnet run`)
- [ ] Swagger доступен на https://localhost:5001/swagger
- [ ] Успешная регистрация пользователя через API

Если все пункты отмечены ✅ - приложение готово к использованию!

---

**Версия документа**: 1.0  
**Дата обновления**: 15 апреля 2026  
**Поддерживаемые платформы**: Windows, macOS, Linux
