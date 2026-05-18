# PostgreSQL и хранение данных в MultiSigSchnorr

## Назначение базы данных

PostgreSQL используется для хранения публичного, административного и отчётного состояния системы:

- эпох;
- участников;
- членства участников в эпохах;
- аудита;
- истории протокольных сессий;
- публичных криптографических артефактов;
- отчётных данных.

Секретные криптографические материалы в PostgreSQL не сохраняются.

---

## Рабочая среда

```text
macOS
Docker Desktop
PostgreSQL в Docker
.NET SDK 10
zsh/bash
```

Корень проекта:

```bash
~/Documents/GitHub/MultiSigSchnorr
```

---

## Параметры PostgreSQL

```text
Container: multisig-postgres
Database:  multisig_schnorr
User:      multisig_user
Password:  multisig_password
Host:      localhost
Port:      5433
```

---

## Запуск

```bash
./scripts/start-postgres.sh
```

Остановка:

```bash
./scripts/stop-postgres.sh
```

Остановка с удалением данных:

```bash
./scripts/stop-postgres.sh --remove-volumes
```

---

## Подключение к базе

```bash
./scripts/open-psql.sh
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

## EF Core

Основной DbContext:

```text
src/MultiSigSchnorr.Infrastructure/Persistence/MultiSigSchnorrDbContext.cs
```

Миграции:

```text
src/MultiSigSchnorr.Infrastructure/Persistence/Migrations
```

Применить миграции:

```bash
./scripts/apply-migrations.sh
```

Создать миграцию:

```bash
./scripts/add-migration.sh MigrationName
```

---

## Основные таблицы

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

## `epochs`

Хранит эпохи протокола.

Пример:

```sql
select id, number, status, created_utc, activated_utc, closed_utc
from epochs
order by number;
```

---

## `participants`

Хранит участников и их публичные ключи.

Приватные ключи здесь не сохраняются.

```sql
select id, display_name, status, created_utc, revoked_utc
from participants
order by display_name;
```

---

## `epoch_members`

Хранит членство участников в эпохах.

```sql
select epoch_id, participant_id, is_active, added_utc, removed_utc
from epoch_members
order by added_utc;
```

---

## `audit_log_entries`

Хранит журнал аудита.

```sql
select id, action_type, entity_type, entity_id, created_utc
from audit_log_entries
order by created_utc desc;
```

---

## `protocol_sessions`

Хранит публичную проекцию протокольной сессии.

```sql
select session_id, session_status, protection_mode, created_utc, completed_utc
from protocol_sessions
order by created_utc desc;
```

---

## `protocol_session_participants`

Хранит публичное состояние участников внутри протокольной сессии.

```sql
select session_id, display_name, has_commitment, has_reveal, has_partial_signature
from protocol_session_participants
order by session_id, display_name;
```

---

## Что сохраняется

```text
Epoch
Participant
EpochMember
AuditLogEntry
ProtocolSession public projection
ProtocolSessionParticipant public projection
```

---

## Что не сохраняется

```text
PrivateKeyMaterial
SecretNonce
Runtime-состояние незавершённой сессии
```

Это ограничение нужно для безопасности: база хранит публичное состояние, но не секреты.

---

## Runtime private key material

Участник может существовать в PostgreSQL, но не иметь приватного ключа в памяти текущего процесса API.

Для этого используется признак:

```text
HasRuntimePrivateKeyMaterial
```

Участник может участвовать в подписи только если:

```text
ParticipantStatus == Active
IsActiveMemberOfActiveEpoch == true
HasRuntimePrivateKeyMaterial == true
```

---

## Диагностика

Endpoint:

```text
GET /api/system/storage
```

Страница:

```text
/system-overview
```

Показывает подключение к БД, количество миграций и количество записей в основных таблицах.
