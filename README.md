## Backend (Mock API)

Сервер по умолчанию запускается на `http://localhost:5000`.

### 1. Запуск через `dotnet run`(.NET 10 SDK)

```bash
cd backend
dotnet restore Project.slnx
dotnet run --project src/Sporim.Api
```

---

### 2. Запуск в Docker

Сборка образа и запуск контейнера из корня репозитория:

```bash
docker build -t sporim-backend ./backend
docker run --rm -p 5000:5000 --name sporim-api sporim-backend
```

---

После запуска сервера доступны:

- **Базовый URL API**: `http://localhost:5000/api/v1`
- **OpenAPI-спецификация**: `http://localhost:5000/openapi/v1.json`
- **Healthcheck**: `http://localhost:5000/health`

### Быстрые dev-токены для заголовка `Authorization`

Для проверки защищённых маршрутов без прохождения полного цикла входа можно передать один из готовых токенов:

- `Authorization: Bearer mock-user-token` — игрок `vanya_analyst` (роль `user`, стартовый баланс `1250`).
- `Authorization: Bearer mock-moderator-token` — модератор `moderator_max` (роль `moderator`).
- `Authorization: Bearer mock-admin-token` — администратор `admin_root` (роль `admin`).

Также работает вход через `POST /api/v1/auth/email/verify-code` (например, email `vanya@sporim.ru` или `moderator@sporim.ru` и любой код кроме `000000`, например `482910`).

### Служебные маршруты для демонстрации сценариев

- `POST /api/v1/mock/reset` — сбросить In-Memory состояние к начальным Seed-данным.
- `POST /api/v1/mock/events/{eventId}/expire-deadline` — принудительно закрыть дедлайн `acceptUntil` у события (для демонстрации сценария `409 EVENT_CLOSED`).
- Заголовок `X-Mock-Delay: 800` — добавить искусственную задержку ответа (в мс) для проверки индикаторов загрузки на клиенте.
