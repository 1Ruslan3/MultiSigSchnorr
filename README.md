# MultiSigSchnorr

## Описание проекта

`MultiSigSchnorr` — программная система для демонстрации и исследования протокола коллективной цифровой подписи на основе алгоритма Шнорра с использованием эллиптических кривых, механизма эпох, управления составом группы подписантов и режима рандомизированной обработки секретного скаляра.

Проект реализует:

- создание протокольной сессии коллективной подписи;
- выбор подписантов из активной эпохи;
- управление участниками и эпохами;
- публикацию commitment-значений;
- раскрытие public nonce;
- формирование частичных подписей;
- агрегирование частичных подписей;
- проверку итоговой коллективной подписи;
- аудит ключевых действий;
- сохранение публичного состояния протокола в PostgreSQL;
- Web-интерфейс для демонстрации.

---

## Текущая рабочая среда

Актуальная среда проекта:

```text
macOS
zsh/bash
Docker Desktop
.NET SDK 10
PostgreSQL через Docker Compose
```

Корень проекта:

```bash
~/Documents/GitHub/MultiSigSchnorr
```

Все команды ниже рассчитаны на macOS.

---

## Используемые технологии

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
- BouncyCastle.Cryptography;
- bash/zsh scripts.

---

## Структура solution

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

tests
├── MultiSigSchnorr.Tests.Unit
├── MultiSigSchnorr.Tests.Integration
├── MultiSigSchnorr.Tests.CryptoVectors
└── MultiSigSchnorr.Benchmarks

deploy  -> Docker Compose и инфраструктурные файлы
docs    -> документация проекта
scripts -> macOS shell-скрипты запуска, тестов и миграций
```

---

## Назначение основных слоёв

### `MultiSigSchnorr.Domain`

Содержит доменные сущности, статусы и value object-типы: участников, эпохи, членство в эпохах, commitment, nonce reveal, частичные подписи, агрегированную подпись и аудит.

### `MultiSigSchnorr.Crypto`

Содержит криптографические сервисы: P-256 curve context, генерацию открытого ключа, агрегирование ключей, challenge, хеширование, nonce, частичную подпись и проверку агрегированной подписи.

### `MultiSigSchnorr.Protocol`

Содержит протокольную логику коллективной подписи: создание сессии, commitment, reveal nonce, partial signatures, aggregate signature и verification.

### `MultiSigSchnorr.Application`

Содержит use cases: создание сессии, выполнение этапов протокола, история, отчёты, аудит, администрирование участников и эпох.

### `MultiSigSchnorr.Infrastructure`

Содержит in-memory репозитории, PostgreSQL-репозитории, EF Core DbContext, persistence entities и миграции.

### `MultiSigSchnorr.Contracts`

Содержит DTO-модели API и Web-клиента.

### `MultiSigSchnorr.Api`

ASP.NET Core Web API.

### `MultiSigSchnorr.Web`

Blazor Web-интерфейс для демонстрации работы системы.

---

## Быстрый запуск на macOS

Перейти в корень проекта:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
```

Сделать shell-скрипты исполняемыми:

```bash
chmod +x scripts/*.sh
```

Запустить PostgreSQL:

```bash
./scripts/start-postgres.sh
```

Применить миграции:

```bash
./scripts/apply-migrations.sh
```

Запустить API:

```bash
./scripts/run-api.sh
```

Во втором терминале запустить Web:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
./scripts/run-web.sh
```

Открыть:

```text
http://localhost:5080/system-overview
```

---

## Ручные команды без скриптов

PostgreSQL:

```bash
docker compose -f deploy/docker-compose.postgres.yml up -d
```

API:

```bash
dotnet run --project src/MultiSigSchnorr.Api/MultiSigSchnorr.Api.csproj
```

Web:

```bash
dotnet run --project src/MultiSigSchnorr.Web/MultiSigSchnorr.Web.csproj
```

---

## PostgreSQL

Контейнер:

```text
multisig-postgres
```

Параметры:

```text
Database: multisig_schnorr
User:     multisig_user
Password: multisig_password
Host:     localhost
Port:     5433
```

Открыть psql:

```bash
./scripts/open-psql.sh
```

Проверить таблицы:

```sql
\dt
```

---

## EF Core migrations

Применить миграции:

```bash
./scripts/apply-migrations.sh
```

Создать миграцию:

```bash
./scripts/add-migration.sh MigrationName
```

Посмотреть миграции:

```bash
./scripts/list-migrations.sh
```

---

## Основные адреса

API:

```text
http://localhost:5227
```

Web:

```text
http://localhost:5080
```

Основные страницы Web:

```text
/system-overview
/administration
/protocol-sessions
/protocol-session-history
/audit-log
```

---

## Основные endpoints

```text
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
GET  /api/admin/epoch-management
POST /api/admin/participants
PUT  /api/admin/participants/{participantId}/display-name
POST /api/admin/participants/{participantId}/revoke
POST /api/admin/epochs/create-with-members
GET  /api/audit
```

---

## Управление составом группы

Состав группы регулируется через механизм эпох:

```text
Participant     -> участник системы
Epoch           -> актуальная конфигурация группы
EpochMember     -> членство участника в эпохе
ProtocolSession -> сессия подписи, созданная по активной эпохе
```

Правильный порядок изменения состава группы:

```text
1. Создать участника.
2. При необходимости переименовать участника.
3. Выбрать участников для новой эпохи.
4. Создать новую активную эпоху с выбранным составом.
5. Создавать новые протокольные сессии уже по новой эпохе.
```

Старые сессии не изменяются, потому что их состав влияет на aggregate public key, challenge и итоговую подпись.

---

## Runtime private key readiness

В PostgreSQL не сохраняются приватные ключи участников.

```text
Participant/PublicKey -> PostgreSQL
PrivateKeyMaterial    -> InMemory
SecretNonce           -> runtime
```

После перезапуска API пользовательские участники остаются в PostgreSQL, но их runtime private key material исчезает. Поэтому система использует признак:

```text
HasRuntimePrivateKeyMaterial
```

Участник доступен для подписи только если:

```text
ParticipantStatus == Active
IsActiveMemberOfActiveEpoch == true
HasRuntimePrivateKeyMaterial == true
```

---

## Запуск тестов

Unit-тесты:

```bash
dotnet test tests/MultiSigSchnorr.Tests.Unit/MultiSigSchnorr.Tests.Unit.csproj
```

Integration-тесты:

```bash
./scripts/start-postgres.sh
dotnet test tests/MultiSigSchnorr.Tests.Integration/MultiSigSchnorr.Tests.Integration.csproj
```

Crypto-vector тесты:

```bash
dotnet test tests/MultiSigSchnorr.Tests.CryptoVectors/MultiSigSchnorr.Tests.CryptoVectors.csproj
```

Все тесты:

```bash
./scripts/test-all.sh
```

Полная проверка сборки и тестов:

```bash
./scripts/build-and-test.sh
```

---

## Запуск бенчмарков

```bash
./scripts/run-benchmarks.sh
```

или вручную:

```bash
dotnet run --project tests/MultiSigSchnorr.Benchmarks/MultiSigSchnorr.Benchmarks.csproj -c Release
```

---

## Что сохраняется в PostgreSQL

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

- приватные ключи участников;
- secret nonce;
- runtime-состояние незавершённой протокольной сессии.

Это сделано намеренно: PostgreSQL хранит публичное и отчётное состояние, но не секретные криптографические материалы.

---

## Документация

```text
docs/ARCHITECTURE.md
docs/DATABASE.md
docs/DEMO_SCENARIO.md
```

---

## Демонстрационный сценарий

Кратко:

```text
1. ./scripts/start-postgres.sh
2. ./scripts/apply-migrations.sh
3. ./scripts/run-api.sh
4. ./scripts/run-web.sh
5. Открыть /system-overview.
6. Открыть /administration.
7. Создать/выбрать участников.
8. Создать новую эпоху.
9. Открыть /protocol-sessions.
10. Создать сессию и пройти протокол подписи.
11. Проверить итоговую подпись.
12. Сформировать отчёт.
13. Открыть /protocol-session-history.
14. Открыть /audit-log.
```

Подробности находятся в `docs/DEMO_SCENARIO.md`.
