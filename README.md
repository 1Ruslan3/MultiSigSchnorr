# MultiSigSchnorr

## Описание проекта

`MultiSigSchnorr` — программная система для демонстрации и исследования протокола коллективной цифровой подписи на основе алгоритма Шнорра с использованием эллиптических кривых и рандомизированной обработки секретного скаляра.

Проект разрабатывается в рамках выпускной квалификационной работы и предназначен для демонстрации следующих возможностей:

- создание протокольной сессии коллективной подписи;
- участие нескольких подписантов в одной подписи;
- публикация commitment-значений;
- раскрытие публичных nonce;
- формирование частичных подписей;
- агрегирование частичных подписей;
- проверка итоговой коллективной подписи;
- сравнение базового режима и режима рандомизированной обработки скаляра;
- ведение аудита ключевых действий;
- сохранение публичного состояния протокола в PostgreSQL;
- просмотр состояния системы через Web-интерфейс.

Проект реализован на платформе `.NET` с разделением на независимые слои: доменная модель, криптографические сервисы, протокольная логика, прикладные сценарии, инфраструктура хранения, API, Web-интерфейс, тесты и бенчмарки.

---

## Используемые технологии

Основные технологии проекта:

- C#;
- .NET 10;
- ASP.NET Core Web API;
- Blazor Web;
- PostgreSQL;
- Docker Compose;
- EF Core;
- Npgsql Entity Framework Core Provider;
- xUnit;
- BenchmarkDotNet;
- BouncyCastle.Cryptography.

---

## Структура solution

Основные проекты находятся в каталоге `src`:

```text
src
├── MultiSigSchnorr.Domain
├── MultiSigSchnorr.Crypto
├── MultiSigSchnorr.Protocol
├── MultiSigSchnorr.Application
├── MultiSigSchnorr.Infrastructure
├── MultiSigSchnorr.Contracts
├── MultiSigSchnorr.Api
└── MultiSigSchnorr.Web
```

Тестовые и исследовательские проекты находятся в каталоге `tests`:

```text
tests
├── MultiSigSchnorr.Tests.Unit
├── MultiSigSchnorr.Tests.Integration
├── MultiSigSchnorr.Tests.CryptoVectors
└── MultiSigSchnorr.Benchmarks
```

Дополнительные каталоги:

```text
deploy  -> Docker Compose и инфраструктурные файлы
docs    -> документация проекта
```

---

## Назначение основных слоёв

### `MultiSigSchnorr.Domain`

Содержит доменные сущности, перечисления и value object-типы:

- участники;
- эпохи;
- членство участников в эпохах;
- commitment-записи;
- nonce reveal;
- частичные подписи;
- агрегированная подпись;
- аудит;
- статусы сессий и участников.

### `MultiSigSchnorr.Crypto`

Содержит криптографические сервисы:

- контекст эллиптической кривой P-256;
- генерация открытого ключа;
- агрегирование открытых ключей;
- вычисление challenge;
- хеширование;
- генерация nonce;
- вычисление частичной подписи;
- проверка агрегированной подписи;
- режим рандомизированной обработки секретного скаляра.

### `MultiSigSchnorr.Protocol`

Содержит протокольную логику:

- создание N-party протокольной сессии;
- публикация commitments;
- раскрытие nonce;
- отправка partial signatures;
- формирование агрегированной подписи;
- проверка участия в активной эпохе;
- отзыв участников;
- переход между эпохами.

### `MultiSigSchnorr.Application`

Содержит прикладные сценарии:

- создание протокольной сессии;
- получение состояния сессии;
- публикация commitment;
- reveal nonce;
- submit partial signature;
- экспорт отчёта;
- получение истории сессий;
- аудит;
- администрирование эпох и участников.

### `MultiSigSchnorr.Infrastructure`

Содержит инфраструктурные реализации:

- in-memory репозитории;
- PostgreSQL/EF Core DbContext;
- PostgreSQL-репозитории;
- persistence-модели;
- миграции EF Core.

### `MultiSigSchnorr.Contracts`

Содержит DTO-модели для обмена между API и Web-клиентом.

### `MultiSigSchnorr.Api`

ASP.NET Core Web API. Предоставляет endpoints для:

- протокольных сессий;
- системной диагностики;
- seed-данных;
- администрирования;
- аудита;
- отчётов.

### `MultiSigSchnorr.Web`

Web-интерфейс для демонстрации работы системы.

---

## PostgreSQL через Docker

PostgreSQL запускается через Docker Compose.

Файл:

```text
deploy/docker-compose.postgres.yml
```

Используется контейнер:

```text
multisig-postgres
```

Параметры базы данных:

```text
Database: multisig_schnorr
User:     multisig_user
Password: multisig_password
Port:     5433
```

Порт `5433` используется на стороне Windows-хоста, чтобы не конфликтовать с возможной локальной установкой PostgreSQL на стандартном порту `5432`.

---

## Запуск PostgreSQL

Из корня проекта:

```powershell
cd C:\Users\user\Desktop\diplom\MultiSigSchnorr

docker compose -f .\deploy\docker-compose.postgres.yml up -d
```

Проверка контейнера:

```powershell
docker ps
```

Просмотр логов:

```powershell
docker logs multisig-postgres
```

Остановка PostgreSQL:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml down
```

Остановка с удалением данных:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml down -v
```

Команда `down -v` удаляет Docker volume с данными PostgreSQL. Использовать только если нужно полностью очистить базу.

---

## Строка подключения

Для локального запуска API используется connection string:

```json
{
  "ConnectionStrings": {
    "MultiSigSchnorrDb": "Host=localhost;Port=5433;Database=multisig_schnorr;Username=multisig_user;Password=multisig_password"
  }
}
```

Файл настройки:

```text
src\MultiSigSchnorr.Api\appsettings.Development.json
```

---

## Миграции EF Core

Создание миграции:

```powershell
dotnet ef migrations add MigrationName `
  --project .\src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj `
  --startup-project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj `
  --context MultiSigSchnorrDbContext `
  --output-dir Persistence\Migrations
```

Применение миграций:

```powershell
dotnet ef database update `
  --project .\src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj `
  --startup-project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj `
  --context MultiSigSchnorrDbContext
```

Просмотр списка миграций:

```powershell
dotnet ef migrations list `
  --project .\src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj `
  --startup-project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj `
  --context MultiSigSchnorrDbContext
```

---

## Проверка таблиц PostgreSQL

Подключение к базе внутри контейнера:

```powershell
docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
```

Просмотр таблиц:

```sql
\dt
```

Ожидаемые таблицы:

```text
__EFMigrationsHistory
audit_log_entries
epochs
participants
epoch_members
protocol_sessions
protocol_session_participants
```

Выход из `psql`:

```sql
\q
```

---

## Запуск API

Из корня проекта:

```powershell
dotnet run --project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj
```

По умолчанию API доступен по адресу:

```text
http://localhost:5227
```

Основные endpoints:

```text
GET  /
GET  /api/system/seed
GET  /api/system/storage
GET  /api/protocol-sessions
POST /api/protocol-sessions
GET  /api/protocol-sessions/{id}
POST /api/protocol-sessions/{id}/commitments
POST /api/protocol-sessions/{id}/reveals
POST /api/protocol-sessions/{id}/partial-signatures
POST /api/protocol-sessions/{id}/verify
GET  /api/protocol-sessions/{id}/report
GET  /api/protocol-sessions/{id}/report.json
GET  /api/protocol-sessions/{id}/report.txt
GET  /api/admin/epoch-management
POST /api/admin/participants/{id}/revoke
POST /api/admin/epochs/transition
GET  /api/audit
```

---

## Запуск Web-интерфейса

Из корня проекта:

```powershell
dotnet run --project .\src\MultiSigSchnorr.Web\MultiSigSchnorr.Web.csproj
```

Web-интерфейс доступен по адресу:

```text
http://localhost:5080
```

Основные страницы:

```text
/system-overview
/protocol-sessions
/protocol-session-history
/administration
/audit-log
```

---

## Сборка проекта

Из корня проекта:

```powershell
dotnet restore
dotnet build
```

---

## Запуск unit-тестов

```powershell
dotnet test .\tests\MultiSigSchnorr.Tests.Unit\MultiSigSchnorr.Tests.Unit.csproj
```

---

## Запуск integration-тестов

Перед запуском integration-тестов должен быть запущен PostgreSQL:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml up -d
```

Затем:

```powershell
dotnet test .\tests\MultiSigSchnorr.Tests.Integration\MultiSigSchnorr.Tests.Integration.csproj
```

---

## Запуск crypto-vector тестов

```powershell
dotnet test .\tests\MultiSigSchnorr.Tests.CryptoVectors\MultiSigSchnorr.Tests.CryptoVectors.csproj
```

---

## Запуск бенчмарков

Бенчмарки запускаются в Release-конфигурации:

```powershell
dotnet run --project .\tests\MultiSigSchnorr.Benchmarks\MultiSigSchnorr.Benchmarks.csproj -c Release
```

Результаты BenchmarkDotNet сохраняются в каталоге:

```text
tests\MultiSigSchnorr.Benchmarks\BenchmarkDotNet.Artifacts
```

---

## Режимы защиты

В проекте реализованы два режима:

### `Baseline`

Базовый режим выполнения операций с секретным скаляром.

### `RandomizedScalarProcessing`

Режим, в котором секретный скаляр обрабатывается через рандомизированное преобразование. Цель режима — повысить устойчивость обработки секретного материала к отдельным классам атак, связанных с анализом вычислительных зависимостей.

В Web-интерфейсе режим выбирается при создании протокольной сессии.

---

## Протокольный цикл

Общий цикл работы коллективной подписи:

```text
1. Создание протокольной сессии.
2. Публикация commitment каждым участником.
3. Раскрытие public nonce каждым участником.
4. Вычисление challenge.
5. Формирование partial signature каждым участником.
6. Агрегирование частичных подписей.
7. Проверка итоговой collective signature.
```

В Web-интерфейсе эти этапы выполняются последовательно на странице:

```text
/protocol-sessions
```

---

## Что сохраняется в PostgreSQL

В PostgreSQL сохраняются:

- эпохи;
- участники;
- членство участников в эпохах;
- аудит;
- публичная проекция протокольных сессий;
- commitment-значения;
- public nonce;
- частичные подписи;
- агрегированная подпись;
- статус сессии;
- режим защиты;
- отчётные данные.

---

## Что не сохраняется в PostgreSQL

В PostgreSQL принципиально не сохраняются:

- приватные ключи участников;
- secret nonce;
- runtime-состояние незавершённой протокольной сессии.

Это сделано намеренно. Система сохраняет публичное и отчётное состояние, но не превращает базу данных в хранилище секретных материалов.

После перезапуска API завершённые сессии доступны для просмотра и экспорта отчётов, но незавершённую runtime-сессию продолжить нельзя.

---

## Текущее состояние persistence

```text
AuditLog                       -> PostgreSQL
Epoch                          -> PostgreSQL
Participant                    -> PostgreSQL
EpochMember                    -> PostgreSQL
ProtocolSession public history -> PostgreSQL projection

ProtocolSession runtime        -> InMemory
PrivateKeyMaterial             -> InMemory
SecretNonce                    -> не сохраняется
```

---

## Диагностика хранилища

Для диагностики PostgreSQL используется endpoint:

```text
GET /api/system/storage
```

Он показывает:

- тип хранилища;
- provider EF Core;
- состояние подключения;
- количество применённых миграций;
- последнюю миграцию;
- количество эпох;
- количество участников;
- количество записей аудита;
- количество протокольных сессий;
- количество участников протокольных сессий.

Эти данные отображаются на странице:

```text
/system-overview
```

---

## Демонстрационный сценарий

Краткий сценарий демонстрации:

1. Запустить PostgreSQL через Docker Compose.
2. Запустить API.
3. Запустить Web.
4. Открыть страницу `/system-overview`.
5. Проверить блок PostgreSQL diagnostics.
6. Перейти на `/protocol-sessions`.
7. Создать протокольную сессию.
8. Выбрать режим `RandomizedScalarProcessing`.
9. Пройти этапы:
   - Publish Commitment;
   - Reveal Nonce;
   - Submit Partial Signature.
10. Проверить итоговую подпись.
11. Сформировать отчёт.
12. Открыть историю сессий.
13. Открыть журнал аудита.
14. Перезапустить API и убедиться, что история и отчёты сохраняются через PostgreSQL-проекцию.

Подробный сценарий находится в файле:

```text
docs\DEMO_SCENARIO.md
```

---

## Документация

Дополнительная документация находится в каталоге `docs`:

```text
docs\ARCHITECTURE.md
docs\DATABASE.md
docs\DEMO_SCENARIO.md
```

### `ARCHITECTURE.md`

Описание архитектуры проекта, слоёв solution и взаимодействия между ними.

### `DATABASE.md`

Описание PostgreSQL-хранилища, таблиц, ограничений и persistence-подхода.

### `DEMO_SCENARIO.md`

Пошаговый сценарий демонстрации проекта на защите.

---

## Назначение проекта в рамках ВКР

Проект демонстрирует практическую реализацию ресурсно-эффективного протокола коллективной цифровой подписи на основе алгоритма Шнорра с использованием рандомизированной обработки секретного скаляра.

Реализация позволяет показать:

- математическую основу протокола;
- структуру многоучастниковой подписи;
- практический backend-контур;
- Web-интерфейс для демонстрации;
- хранение публичного состояния протокола;
- аудит действий;
- экспериментальную оценку накладных расходов защитного режима.

---

## Рекомендуемый порядок запуска

Для обычной демонстрации:

```powershell
cd C:\Users\user\Desktop\diplom\MultiSigSchnorr
docker compose -f .\deploy\docker-compose.postgres.yml up -d
dotnet run --project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj
```

В отдельном окне PowerShell:

```powershell
cd C:\Users\user\Desktop\diplom\MultiSigSchnorr
dotnet run --project .\src\MultiSigSchnorr.Web\MultiSigSchnorr.Web.csproj
```

После запуска открыть:

```text
http://localhost:5080/system-overview
```

---

## Рекомендуемый порядок проверки перед защитой

```powershell
dotnet restore
dotnet build
dotnet test .\tests\MultiSigSchnorr.Tests.Unit\MultiSigSchnorr.Tests.Unit.csproj
dotnet test .\tests\MultiSigSchnorr.Tests.Integration\MultiSigSchnorr.Tests.Integration.csproj
dotnet test .\tests\MultiSigSchnorr.Tests.CryptoVectors\MultiSigSchnorr.Tests.CryptoVectors.csproj
```

Для бенчмарков:

```powershell
dotnet run --project .\tests\MultiSigSchnorr.Benchmarks\MultiSigSchnorr.Benchmarks.csproj -c Release
```

---

## Ключевая идея реализации

Главная идея реализации заключается в разделении публичного и секретного состояния:

```text
Публичное состояние протокола -> PostgreSQL
Секретное runtime-состояние   -> память процесса
```

Такой подход позволяет сохранять историю, отчёты и аудит, но не хранить приватные ключи и secret nonce в базе данных.

Это важно для демонстрации того, что проект учитывает не только функциональность, но и требования безопасности криптографической системы.
