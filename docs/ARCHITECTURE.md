# Архитектура проекта MultiSigSchnorr

## Общая характеристика

`MultiSigSchnorr` построен по многослойной архитектуре:

```text
Domain          -> предметная область
Crypto          -> криптографические операции
Protocol        -> протокольная логика коллективной подписи
Application     -> сценарии использования
Infrastructure  -> хранение и внешние зависимости
Contracts       -> DTO-модели обмена
Api             -> HTTP API
Web             -> пользовательский интерфейс
Tests           -> автоматическая проверка
Benchmarks      -> измерение производительности
```

---

## Схема верхнего уровня

```text
Пользователь
    |
    v
MultiSigSchnorr.Web
    |
    v
MultiSigSchnorr.Api
    |
    v
MultiSigSchnorr.Application
    |
    +------------------+
    |                  |
    v                  v
MultiSigSchnorr.Protocol     MultiSigSchnorr.Infrastructure
    |                                  |
    v                                  v
MultiSigSchnorr.Crypto          PostgreSQL / InMemory
    |
    v
MultiSigSchnorr.Domain
```

---

## Domain

Содержит сущности и value objects:

- `Participant`;
- `Epoch`;
- `EpochMember`;
- commitment;
- nonce reveal;
- partial signature;
- aggregate signature;
- audit event;
- статусы и режимы защиты.

Domain не зависит от API, Web или EF Core.

---

## Crypto

Содержит криптографические сервисы:

- P-256 curve context;
- public key generation;
- aggregate key service;
- hash-to-scalar;
- challenge service;
- nonce generation;
- partial signature service;
- aggregate signature verifier;
- randomized scalar processing.

---

## Protocol

Реализует полный протокольный цикл:

```text
1. Create Session
2. Publish Commitments
3. Reveal Nonces
4. Submit Partial Signatures
5. Build Aggregate Signature
6. Verify Aggregate Signature
```

---

## Управление составом группы

Состав группы задаётся не редактированием старых сессий, а созданием новых эпох:

```text
Participant        -> участник системы
Epoch              -> конфигурация группы
EpochMember        -> участник в эпохе
ProtocolSession    -> сессия подписи по эпохе
```

Если состав группы меняется, создаётся новая эпоха. Старые сессии остаются связанными со старой эпохой.

---

## Application

Содержит use cases:

- создание протокольной сессии;
- commitment;
- reveal nonce;
- partial signature;
- verify;
- report;
- history;
- audit;
- create participant;
- rename participant;
- revoke participant;
- create epoch with members;
- get administration state.

---

## Infrastructure

Содержит:

- in-memory репозитории;
- PostgreSQL-репозитории;
- EF Core DbContext;
- persistence entities;
- migrations.

PostgreSQL хранит публичное и отчётное состояние. Секреты хранятся только runtime.

---

## Contracts

Содержит DTO для API и Web.

Это позволяет не отдавать наружу доменные сущности напрямую.

---

## API

Основные группы endpoints:

```text
/api/system
/api/system/storage
/api/protocol-sessions
/api/admin
/api/audit
```

API предоставляет управление протоколом, участниками, эпохами, аудитом и отчётами.

---

## Web

Основные страницы:

```text
/system-overview
/administration
/protocol-sessions
/protocol-session-history
/audit-log
```

`Administration` управляет группой и эпохами.

`Protocol Sessions` создаёт сессии по активной эпохе и выбранным участникам.

---

## Persistence-подход

```text
Публичное и отчётное состояние -> PostgreSQL
Секретное runtime-состояние    -> InMemory
```

PostgreSQL хранит:

- epochs;
- participants;
- epoch members;
- audit log entries;
- protocol session public projection;
- protocol session participants projection.

PostgreSQL не хранит:

- private key material;
- secret nonce;
- runtime-состояние незавершённой сессии.

---

## Runtime private key readiness

Система явно отслеживает наличие приватного ключа в памяти процесса API:

```text
HasRuntimePrivateKeyMaterial
```

Это предотвращает выбор участника, который есть в PostgreSQL, но не может подписывать после перезапуска API.

---

## Тестирование

- Unit-тесты проверяют отдельные компоненты.
- Integration-тесты проверяют API, PostgreSQL и group management.
- Crypto-vector тесты проверяют стабильность криптографических value object.
- Benchmarks измеряют накладные расходы защитного режима.

---

## Итоговая идея

Проект разделяет:

```text
1. Доменную модель.
2. Runtime-состояние выполнения протокола.
3. PostgreSQL-проекцию для истории, отчётов и аудита.
```

Это позволяет показывать полноценную систему, не сохраняя секретные криптографические материалы в базе данных.
