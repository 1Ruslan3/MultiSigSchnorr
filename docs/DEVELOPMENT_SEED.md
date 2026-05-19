# Development seed в MultiSigSchnorr

## Назначение

`DevelopmentDataSeeder` используется только для локальной разработки, тестирования и демонстрационного стенда.

Он выполняет две задачи:

1. Создаёт фиксированных seed-участников, если их ещё нет в PostgreSQL.
2. Восстанавливает runtime private key material для этих seed-участников в памяти API.

Это позволяет быстро запустить приложение и проверить работу протокола без ручного создания начального состава группы.

---

## Почему seed ограничен средой Development

В seed-контуре используются фиксированные демонстрационные приватные ключи.

Это допустимо для локального стенда, но недопустимо как production-механизм.

Поэтому endpoint:

```text
GET /api/system/seed
```

доступен только в среде:

```text
Development
```

Если приложение запущено в другой среде, endpoint возвращает `404 Not Found`.

---

## Что seed НЕ делает

`DevelopmentDataSeeder` не является механизмом управления группой.

После появления полноценного управления участниками и эпохами seed не должен менять пользовательский состав активной эпохи.

Текущая логика:

```text
Если активная эпоха уже существует:
    seed не добавляет туда seed-участников

Если активной эпохи нет:
    seed создаёт начальную development-эпоху
```

---

## Основное управление группой

Основное управление составом группы выполняется через:

```text
/administration
```

и API endpoints:

```text
POST /api/admin/participants
PUT  /api/admin/participants/{participantId}/display-name
POST /api/admin/participants/{participantId}/revoke
POST /api/admin/epochs/create-with-members
POST /api/admin/demo-group
```

Для демонстрации на защите рекомендуется использовать:

```text
POST /api/admin/demo-group
```

или кнопку создания демо-группы на странице `Administration`.

---

## Что сохраняется

Seed-участники сохраняются в PostgreSQL как обычные публичные участники:

```text
Participant/PublicKey -> PostgreSQL
```

Их приватные ключи не сохраняются в PostgreSQL:

```text
PrivateKeyMaterial -> InMemory
```

После перезапуска API `DevelopmentDataSeeder` может восстановить runtime private key material только для фиксированных seed-участников.

Для участников, созданных пользователем, private key material не восстанавливается автоматически после перезапуска API.

---

## Как объяснять в ВКР

Seed-контур является вспомогательным development-механизмом. Он не относится к промышленному хранению ключей.

В основной модели безопасности:

- PostgreSQL хранит публичное и отчётное состояние;
- приватные ключи не сохраняются в базе данных;
- secret nonce не сохраняются в базе данных;
- runtime key readiness отображается отдельно;
- участник может подписывать только при наличии runtime private key material.
