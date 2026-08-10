# API Usage Guide - Руководство по использованию API

## 🔐 Аутентификация

### 1. Регистрация нового пользователя

**Endpoint**: `POST /api/auth/register`

**Request Body**:
```json
{
  "email": "researcher@lab.com",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "fullName": "Иван Петров",
  "position": "Старший научный сотрудник"
}
```

**Response** (201 Created):
```json
{
  "message": "Пользователь успешно зарегистрирован",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Правила для пароля**:
- Минимум 8 символов
- Хотя бы одна заглавная буква
- Хотя бы одна строчная буква
- Хотя бы одна цифра

### 2. Вход в систему

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "researcher@lab.com",
  "password": "SecurePass123"
}
```

**Response** (200 OK):
```json
{
  "message": "Успешный вход",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "researcher@lab.com",
    "fullName": "Иван Петров",
    "position": "Старший научный сотрудник",
    "roles": ["Researcher"]
  }
}
```

### 3. Получение информации о текущем пользователе

**Endpoint**: `GET /api/auth/me`

**Headers**:
```
Cookie: .AspNetCore.Identity.Application=...
```

**Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "researcher@lab.com",
  "fullName": "Иван Петров",
  "position": "Старший научный сотрудник",
  "roles": ["Researcher"],
  "isActive": true,
  "lastLoginAt": "2026-04-15T10:30:00Z"
}
```

## 🔬 Управление экспериментами

### 1. Получить список экспериментов

**Endpoint**: `GET /api/experiments?pageNumber=1&pageSize=10&searchTerm=&status=0`

**Query Parameters**:
- `pageNumber` (int, default: 1) - Номер страницы
- `pageSize` (int, default: 10) - Размер страницы
- `searchTerm` (string, optional) - Поисковый запрос
- `status` (int, optional) - Статус эксперимента (0=Planning, 1=InProgress, 2=Completed, 3=Suspended, 4=Cancelled)

**Response** (200 OK):
```json
{
  "total": 5,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 1,
      "title": "Анализ воды",
      "description": "Комплексный анализ образцов воды",
      "objective": "Определить качество воды",
      "status": 1,
      "startDate": "2026-04-15T00:00:00Z",
      "endDate": "2026-05-15T00:00:00Z",
      "createdAt": "2026-04-15T10:00:00Z",
      "updatedAt": "2026-04-15T10:30:00Z",
      "resultsCount": 3,
      "entriesCount": 5
    }
  ]
}
```

### 2. Создать новый эксперимент

**Endpoint**: `POST /api/experiments`

**Request Body**:
```json
{
  "title": "Анализ воды",
  "description": "Комплексный анализ образцов воды",
  "objective": "Определить качество воды",
  "methodology": "Титрование и спектрофотометрия",
  "startDate": "2026-04-15",
  "endDate": "2026-05-15"
}
```

**Response** (201 Created):
```json
{
  "id": 1,
  "title": "Анализ воды",
  "status": 0,
  "createdAt": "2026-04-15T10:00:00Z"
}
```

### 3. Получить информацию об эксперименте

**Endpoint**: `GET /api/experiments/{id}`

**Response** (200 OK):
```json
{
  "id": 1,
  "title": "Анализ воды",
  "description": "Комплексный анализ образцов воды",
  "objective": "Определить качество воды",
  "methodology": "Титрование и спектрофотометрия",
  "status": 1,
  "startDate": "2026-04-15T00:00:00Z",
  "endDate": "2026-05-15T00:00:00Z",
  "createdAt": "2026-04-15T10:00:00Z",
  "updatedAt": "2026-04-15T10:30:00Z",
  "results": 3,
  "entries": 5
}
```

### 4. Обновить эксперимент

**Endpoint**: `PUT /api/experiments/{id}`

**Request Body** (все поля опциональны):
```json
{
  "title": "Анализ воды (обновлено)",
  "status": 2,
  "endDate": "2026-05-20"
}
```

**Response** (200 OK):
```json
{
  "message": "Эксперимент успешно обновлён"
}
```

### 5. Удалить эксперимент

**Endpoint**: `DELETE /api/experiments/{id}`

**Response** (200 OK):
```json
{
  "message": "Эксперимент успешно удалён"
}
```

## 📝 Записи в журнале

### 1. Получить записи эксперимента

**Endpoint**: `GET /api/journalentries/experiment/{experimentId}?pageNumber=1&pageSize=20`

**Response** (200 OK):
```json
{
  "total": 3,
  "pageNumber": 1,
  "pageSize": 20,
  "data": [
    {
      "id": 1,
      "title": "Первое наблюдение",
      "content": "Заметил изменение цвета раствора...",
      "type": 0,
      "priority": 1,
      "tags": "наблюдение,изменение",
      "createdAt": "2026-04-15T10:15:00Z",
      "updatedAt": "2026-04-15T10:15:00Z",
      "author": "Иван Петров"
    }
  ]
}
```

### 2. Создать запись в журнале

**Endpoint**: `POST /api/journalentries`

**Request Body**:
```json
{
  "experimentId": 1,
  "title": "Первое наблюдение",
  "content": "Заметил изменение цвета раствора на синий. Температура повысилась на 2°C",
  "type": 0,
  "priority": 1,
  "tags": "наблюдение,изменение,важно",
  "attachments": "link_to_photo"
}
```

**Type枚举**:
- 0 = Observation (Наблюдение)
- 1 = Result (Результат)
- 2 = Problem (Проблема)
- 3 = Note (Заметка)
- 4 = Procedure (Процедура)
- 5 = Conclusion (Вывод)

**Priority枚举**:
- 0 = Low (Низкий)
- 1 = Normal (Обычный)
- 2 = High (Высокий)
- 3 = Critical (Критичный)

**Response** (201 Created):
```json
{
  "id": 1,
  "title": "Первое наблюдение",
  "type": 0,
  "createdAt": "2026-04-15T10:15:00Z"
}
```

### 3. Поиск записей

**Endpoint**: `GET /api/journalentries/search?query=наблюдение`

**Response** (200 OK):
```json
{
  "total": 2,
  "data": [
    {
      "id": 1,
      "title": "Первое наблюдение",
      "content": "Заметил изменение цвета раствора...",
      "type": 0,
      "tags": "наблюдение,изменение",
      "createdAt": "2026-04-15T10:15:00Z",
      "experiment": "Анализ воды",
      "author": "Иван Петров"
    }
  ]
}
```

## 📊 Результаты экспериментов

### 1. Создать результат

**Endpoint**: `POST /api/experimentresults`

**Request Body**:
```json
{
  "experimentId": 1,
  "name": "pH раствора",
  "description": "Измерение pH при помощи pH-метра",
  "value": "7.5",
  "unit": "pH",
  "dataType": 0,
  "notes": "Результат соответствует норме",
  "recordedAt": "2026-04-15T11:00:00Z"
}
```

**DataType枚举**:
- 0 = Numeric (Числовое)
- 1 = Text (Текстовое)
- 2 = Boolean (Логическое)
- 3 = File (Файл)
- 4 = Image (Изображение)
- 5 = Other (Другое)

**Response** (201 Created):
```json
{
  "id": 1,
  "name": "pH раствора",
  "value": "7.5",
  "status": 0,
  "createdAt": "2026-04-15T11:00:00Z"
}
```

### 2. Получить результаты эксперимента

**Endpoint**: `GET /api/experimentresults/experiment/{experimentId}`

**Response** (200 OK):
```json
{
  "total": 2,
  "pageNumber": 1,
  "pageSize": 20,
  "data": [
    {
      "id": 1,
      "name": "pH раствора",
      "description": "Измерение pH",
      "value": "7.5",
      "unit": "pH",
      "dataType": 0,
      "status": 1,
      "recordedAt": "2026-04-15T11:00:00Z",
      "createdAt": "2026-04-15T11:00:00Z"
    }
  ]
}
```

### 3. Получить статистику

**Endpoint**: `GET /api/experimentresults/experiment/{experimentId}/statistics`

**Response** (200 OK):
```json
{
  "totalResults": 5,
  "byStatus": {
    "pending": 2,
    "verified": 2,
    "approved": 1,
    "rejected": 0,
    "archived": 0
  },
  "byDataType": {
    "numeric": 3,
    "text": 1,
    "boolean": 0,
    "file": 1,
    "image": 0
  }
}
```

## 🔍 Поиск

### 1. Глобальный поиск

**Endpoint**: `GET /api/search/global?query=анализ&pageNumber=1&pageSize=10`

**Response** (200 OK):
```json
{
  "query": "анализ",
  "total": 3,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "type": "Experiment",
      "id": 1,
      "title": "Анализ воды",
      "description": "Комплексный анализ образцов воды",
      "createdAt": "2026-04-15T10:00:00Z",
      "score": 100
    }
  ]
}
```

### 2. Расширенный поиск (POST)

**Endpoint**: `POST /api/search/experiments`

**Request Body**:
```json
{
  "title": "анализ",
  "status": 1,
  "startDateFrom": "2026-04-01",
  "startDateTo": "2026-05-01"
}
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "title": "Анализ воды",
    "description": "Комплексный анализ образцов воды",
    "objective": "Определить качество воды",
    "status": 1,
    "startDate": "2026-04-15T00:00:00Z",
    "endDate": "2026-05-15T00:00:00Z",
    "createdAt": "2026-04-15T10:00:00Z"
  }
]
```

### 3. Получить рекомендации

**Endpoint**: `GET /api/search/recommendations`

**Response** (200 OK):
```json
{
  "recentExperiments": [
    {
      "type": "Experiment",
      "id": 1,
      "title": "Анализ воды",
      "updatedAt": "2026-04-15T10:30:00Z"
    }
  ],
  "activeExperiments": [
    {
      "type": "Active",
      "id": 1,
      "title": "Анализ воды",
      "startDate": "2026-04-15T00:00:00Z"
    }
  ],
  "recentEntries": [
    {
      "type": "JournalEntry",
      "id": 1,
      "title": "Первое наблюдение",
      "createdAt": "2026-04-15T10:15:00Z"
    }
  ]
}
```

## ⚠️ Коды ошибок

| Код | Описание |
|-----|---------|
| 200 | OK - Успешный запрос |
| 201 | Created - Ресурс создан |
| 400 | Bad Request - Неверный формат запроса |
| 401 | Unauthorized - Требуется аутентификация |
| 403 | Forbidden - Доступ запрещён |
| 404 | Not Found - Ресурс не найден |
| 500 | Internal Server Error - Ошибка сервера |

## 📝 Примеры cURL команд

### Регистрация:
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@lab.com",
    "password": "Test@123456",
    "confirmPassword": "Test@123456",
    "fullName": "Test User",
    "position": "Researcher"
  }'
```

### Вход:
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "email": "test@lab.com",
    "password": "Test@123456"
  }'
```

### Создание эксперимента:
```bash
curl -X POST https://localhost:5001/api/experiments \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "title": "Test Experiment",
    "description": "Test",
    "objective": "Testing",
    "methodology": "Manual",
    "startDate": "2026-04-15"
  }'
```

---

**Дата**: 15 апреля 2026  
**Версия API**: 1.0
