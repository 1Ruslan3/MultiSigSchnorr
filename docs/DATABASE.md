# PostgreSQL и хранение данных в MultiSigSchnorr

## Назначение базы данных

В проекте `MultiSigSchnorr` PostgreSQL используется для хранения публичного, административного и отчётного состояния системы.

База данных нужна для того, чтобы после перезапуска API сохранялись:

- эпохи;
- участники;
- членство участников в эпохах;
- журнал аудита;
- история протокольных сессий;
- публичные криптографические артефакты;
- отчётные данные.

При этом база данных не используется для хранения секретных криптографических материалов.

---

## Почему используется PostgreSQL

PostgreSQL выбран как надёжная реляционная СУБД, подходящая для хранения структурированных данных:

- участников;
- эпох;
- протокольных сессий;
- аудита;
- отчётных записей.

Использование PostgreSQL позволяет:

- сохранять данные между запусками приложения;
- выполнять диагностику состояния системы;
- анализировать историю протокольных сессий;
- демонстрировать полноценную backend-инфраструктуру в ВКР;
- использовать миграции EF Core для управления схемой БД.

---

## Почему PostgreSQL запускается через Docker

В проекте PostgreSQL запускается через Docker Compose.

Такой подход удобен, потому что:

- не требуется устанавливать PostgreSQL напрямую в Windows;
- база поднимается одной командой;
- окружение легко воспроизвести;
- данные сохраняются в Docker volume;
- конфигурация хранится в проекте;
- можно быстро очистить базу при необходимости.

Файл Docker Compose:

```text
deploy/docker-compose.postgres.yml
```

---

## Параметры контейнера

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

Внутри контейнера PostgreSQL работает на стандартном порту `5432`.

На стороне Windows используется порт `5433`, чтобы избежать конфликта с локально установленным PostgreSQL.

---

## Запуск PostgreSQL

Из корня проекта:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml up -d
```

Проверка:

```powershell
docker ps
```

Просмотр логов:

```powershell
docker logs multisig-postgres
```

Остановка:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml down
```

Остановка с удалением данных:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml down -v
```

---

## Подключение к PostgreSQL вручную

```powershell
docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
```

Просмотр таблиц:

```sql
\dt
```

Выход:

```sql
\q
```

---

## Connection string

Для локального запуска API используется строка подключения:

```json
{
  "ConnectionStrings": {
    "MultiSigSchnorrDb": "Host=localhost;Port=5433;Database=multisig_schnorr;Username=multisig_user;Password=multisig_password"
  }
}
```

Файл:

```text
src\MultiSigSchnorr.Api\appsettings.Development.json
```

---

## EF Core

Для работы с PostgreSQL используется EF Core и провайдер Npgsql.

Основной DbContext:

```text
src\MultiSigSchnorr.Infrastructure\Persistence\MultiSigSchnorrDbContext.cs
```

Миграции находятся в каталоге:

```text
src\MultiSigSchnorr.Infrastructure\Persistence\Migrations
```

---

## Создание миграции

```powershell
dotnet ef migrations add MigrationName `
  --project .\src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj `
  --startup-project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj `
  --context MultiSigSchnorrDbContext `
  --output-dir Persistence\Migrations
```

---

## Применение миграций

```powershell
dotnet ef database update `
  --project .\src\MultiSigSchnorr.Infrastructure\MultiSigSchnorr.Infrastructure.csproj `
  --startup-project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj `
  --context MultiSigSchnorrDbContext
```

---

## Таблицы базы данных

В текущей версии используются следующие таблицы:

```text
__EFMigrationsHistory
epochs
participants
epoch_members
audit_log_entries
protocol_sessions
protocol_session_participants
```

---

## `__EFMigrationsHistory`

Служебная таблица EF Core.

Хранит информацию о применённых миграциях:

- MigrationId;
- ProductVersion.

Пример запроса:

```sql
select * from "__EFMigrationsHistory";
```

---

## `epochs`

Таблица хранит эпохи протокола.

Эпоха определяет актуальный состав участников, которые могут участвовать в коллективной подписи.

Основные поля:

```text
id
number
status
created_utc
activated_utc
closed_utc
```

Пример запроса:

```sql
select id, number, status, created_utc, activated_utc, closed_utc
from epochs
order by number;
```

---

## `participants`

Таблица хранит участников протокола.

Основные поля:

```text
id
display_name
public_key_hex
status
created_utc
revoked_utc
```

В таблице хранится только публичный ключ участника. Приватный ключ в PostgreSQL не сохраняется.

Пример запроса:

```sql
select id, display_name, status, created_utc, revoked_utc
from participants
order by display_name;
```

---

## `epoch_members`

Таблица хранит связь участников с эпохами.

Основные поля:

```text
id
epoch_id
participant_id
added_utc
removed_utc
is_active
```

Эта таблица позволяет определить, какие участники входят в конкретную эпоху.

Пример запроса:

```sql
select epoch_id, participant_id, is_active, added_utc, removed_utc
from epoch_members
order by added_utc;
```

---

## `audit_log_entries`

Таблица хранит журнал аудита.

Основные поля:

```text
id
action_type
entity_type
entity_id
description
metadata_json
created_utc
```

В аудит записываются ключевые действия:

- создание протокольной сессии;
- отзыв участника;
- переход к следующей эпохе.

Пример запроса:

```sql
select id, action_type, entity_type, entity_id, created_utc
from audit_log_entries
order by created_utc desc;
```

---

## `protocol_sessions`

Таблица хранит публичную проекцию протокольной сессии.

Основные поля:

```text
session_id
epoch_id
epoch_number
session_status
protection_mode
created_utc
completed_utc
message_digest_hex
aggregate_public_key_hex
aggregate_nonce_point_hex
challenge_hex
aggregate_signature_nonce_point_hex
aggregate_signature_scalar_hex
all_commitments_published
all_nonces_revealed
all_partial_signatures_submitted
```

Эта таблица нужна для:

- истории протокольных сессий;
- отчётов;
- просмотра публичных криптографических артефактов;
- демонстрации результата после перезапуска API.

Пример запроса:

```sql
select session_id, session_status, protection_mode, created_utc, completed_utc
from protocol_sessions
order by created_utc desc;
```

---

## `protocol_session_participants`

Таблица хранит публичное состояние участников внутри протокольной сессии.

Основные поля:

```text
id
session_id
participant_id
display_name
has_commitment
has_reveal
has_partial_signature
public_key_hex
aggregation_coefficient_hex
commitment_hex
public_nonce_point_hex
partial_signature_hex
```

Таблица позволяет посмотреть состояние каждого участника в конкретной сессии.

Пример запроса:

```sql
select session_id, display_name, has_commitment, has_reveal, has_partial_signature
from protocol_session_participants
order by session_id, display_name;
```

---

## Что сохраняется в PostgreSQL

В PostgreSQL сохраняется только публичное и отчётное состояние.

Сохраняются:

```text
Epoch
Participant
EpochMember
AuditLogEntry
ProtocolSession public projection
ProtocolSessionParticipant public projection
```

Для протокольной сессии сохраняются:

- идентификатор сессии;
- идентификатор эпохи;
- номер эпохи;
- статус сессии;
- режим защиты;
- digest сообщения;
- aggregate public key;
- aggregate nonce point;
- challenge;
- aggregate signature;
- commitment;
- public nonce;
- partial signature;
- флаги прохождения этапов.

---

## Что не сохраняется в PostgreSQL

В PostgreSQL не сохраняются:

```text
PrivateKeyMaterial
SecretNonce
Runtime-состояние незавершённой сессии
```

Это важное ограничение безопасности.

Приватные ключи и secret nonce не должны сохраняться в базе данных в открытом виде. Поэтому PostgreSQL используется для истории и отчётности, но не для хранения секретных криптографических материалов.

---

## Почему используется projection-модель

Протокольная сессия в runtime содержит не только публичные данные, но и чувствительное состояние.

Если сохранить её целиком, в БД могли бы попасть:

- приватные ключи;
- secret nonce;
- промежуточные секретные значения.

Поэтому используется отдельная projection-модель:

```text
ProtocolSessionProjection
ProtocolSessionParticipantProjection
```

Она содержит только публичные и отчётные данные.

---

## Последствия такого подхода

После перезапуска API:

- история сессий сохраняется;
- отчёты доступны;
- публичные криптографические артефакты доступны;
- аудит сохраняется;
- эпохи и участники сохраняются.

Но:

- незавершённую runtime-сессию нельзя продолжить;
- secret nonce не восстанавливается;
- приватные ключи не читаются из PostgreSQL.

Это осознанное архитектурное ограничение.

---

## Runtime-хранилище

Некоторые данные остаются в памяти процесса:

```text
IProtocolSessionRepository -> InMemoryProtocolSessionRepository
IPrivateKeyMaterialRepository -> InMemoryPrivateKeyMaterialRepository
```

Это необходимо для безопасного выполнения протокола во время работы API.

---

## Текущее состояние хранения

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

Для проверки состояния БД реализован endpoint:

```text
GET /api/system/storage
```

Он возвращает:

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

Эти данные также отображаются на странице:

```text
/system-overview
```

---

## Примеры полезных SQL-запросов

### Проверка миграций

```sql
select * from "__EFMigrationsHistory";
```

### Проверка эпох

```sql
select id, number, status, created_utc, activated_utc, closed_utc
from epochs
order by number;
```

### Проверка участников

```sql
select id, display_name, status, created_utc, revoked_utc
from participants
order by display_name;
```

### Проверка членства в эпохах

```sql
select epoch_id, participant_id, is_active, added_utc, removed_utc
from epoch_members
order by added_utc;
```

### Проверка аудита

```sql
select id, action_type, entity_type, entity_id, created_utc
from audit_log_entries
order by created_utc desc;
```

### Проверка протокольных сессий

```sql
select session_id, session_status, protection_mode, created_utc, completed_utc
from protocol_sessions
order by created_utc desc;
```

### Проверка участников протокольных сессий

```sql
select session_id, display_name, has_commitment, has_reveal, has_partial_signature
from protocol_session_participants
order by session_id, display_name;
```

---

## Значение для ВКР

Использование PostgreSQL показывает, что проект является не только демонстрацией криптографических операций, но и полноценной backend-системой с сохраняемым состоянием.

При этом архитектура учитывает требования безопасности:

- публичные данные сохраняются;
- секретные данные не сохраняются;
- отчёты и аудит доступны после перезапуска;
- незавершённые сессии не восстанавливаются без секретного runtime-состояния.

Такой подход позволяет обосновать проект как практическую реализацию протокола коллективной цифровой подписи с осознанным разделением публичного и секретного состояния.
