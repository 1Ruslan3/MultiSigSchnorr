# Демонстрационный сценарий MultiSigSchnorr на macOS

## Назначение

Документ описывает последовательность действий для демонстрации проекта `MultiSigSchnorr` на macOS.

Сценарий показывает:

- запуск PostgreSQL через Docker;
- запуск API и Web;
- диагностику хранилища;
- управление участниками и эпохами;
- создание протокольной сессии;
- полный цикл коллективной подписи;
- проверку итоговой подписи;
- отчётность, историю и аудит.

---

## 1. Подготовка

Перейти в корень проекта:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
```

Сделать скрипты исполняемыми:

```bash
chmod +x scripts/*.sh
```

---

## 2. Запуск PostgreSQL

```bash
./scripts/start-postgres.sh
```

Проверить контейнер:

```bash
docker ps
```

Ожидаемый контейнер:

```text
multisig-postgres
```

---

## 3. Применение миграций

```bash
./scripts/apply-migrations.sh
```

---

## 4. Проверка PostgreSQL

```bash
./scripts/open-psql.sh
```

Внутри psql:

```sql
\dt
```

Выйти:

```sql
\q
```

---

## 5. Запуск API

В отдельном терминале:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
./scripts/run-api.sh
```

API должен быть доступен по адресу:

```text
http://localhost:5227
```

Проверка:

```bash
curl http://localhost:5227
```

---

## 6. Запуск Web

Во втором терминале:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
./scripts/run-web.sh
```

Web-интерфейс:

```text
http://localhost:5080
```

---

## 7. System Overview

Открыть:

```text
http://localhost:5080/system-overview
```

Показать:

- активную эпоху;
- seed-участников;
- PostgreSQL diagnostics;
- состояние подключения;
- количество миграций;
- количество записей в таблицах.

---

## 8. Управление группой

Открыть:

```text
http://localhost:5080/administration
```

Показать:

- список участников;
- статусы участников;
- runtime key readiness;
- текущую активную эпоху;
- историю эпох.

Демонстрационные действия:

```text
1. Создать нового участника.
2. Переименовать участника.
3. Выбрать несколько участников с Runtime key ready.
4. Создать новую эпоху с выбранным составом.
5. Убедиться, что номер активной эпохи увеличился.
```

---

## 9. Создание протокольной сессии

Открыть:

```text
http://localhost:5080/protocol-sessions
```

Показать:

- активную эпоху;
- доступных участников активной эпохи;
- выбор подписантов;
- режим защиты;
- поле сообщения.

Выбрать минимум двух участников.

Режим защиты:

```text
RandomizedScalarProcessing
```

Нажать:

```text
Создать протокольную сессию
```

---

## 10. Этап commitment

Для каждого выбранного участника нажать:

```text
Publish Commitment
```

После выполнения у всех участников должно быть:

```text
HasCommitment = true
AllCommitmentsPublished = true
```

---

## 11. Этап reveal nonce

Для каждого участника нажать:

```text
Reveal Nonce
```

После выполнения должны появиться:

- public nonce point участников;
- aggregate nonce point;
- challenge.

---

## 12. Этап partial signatures

Для каждого участника нажать:

```text
Submit Partial Signature
```

После выполнения:

```text
AllPartialSignaturesSubmitted = true
SessionStatus = Completed
```

Должны появиться:

- aggregate signature nonce point;
- aggregate signature scalar.

---

## 13. Проверка подписи

Нажать:

```text
Проверить итоговую подпись
```

Ожидаемый результат:

```text
Aggregate signature is valid.
```

---

## 14. Отчёт

Нажать:

```text
Сформировать отчёт
```

Показать:

- статус сессии;
- режим защиты;
- количество участников;
- digest сообщения;
- aggregate public key;
- aggregate signature;
- состояние этапов.

Также показать скачивание JSON/TXT отчёта.

---

## 15. История сессий

Открыть:

```text
http://localhost:5080/protocol-session-history
```

Показать созданную сессию и её параметры.

---

## 16. Audit Log

Открыть:

```text
http://localhost:5080/audit-log
```

Показать журнал аудита и фильтрацию.

---

## 17. Проверка PostgreSQL-проекции

Открыть psql:

```bash
./scripts/open-psql.sh
```

Проверить сессии:

```sql
select session_id, session_status, protection_mode, created_utc, completed_utc
from protocol_sessions
order by created_utc desc;
```

Проверить участников сессий:

```sql
select session_id, display_name, has_commitment, has_reveal, has_partial_signature
from protocol_session_participants
order by session_id, display_name;
```

---

## 18. Проверка сохранения после перезапуска API

1. Остановить API через `Ctrl + C`.
2. Запустить API заново:

```bash
./scripts/run-api.sh
```

3. Открыть:

```text
http://localhost:5080/protocol-session-history
```

Сессия должна остаться в истории, потому что публичная проекция хранится в PostgreSQL.

---

## 19. Бенчмарки

Запуск:

```bash
./scripts/run-benchmarks.sh
```

Показать сравнение режимов:

- Baseline;
- RandomizedScalarProcessing.

---

## 20. Полная проверка перед защитой

```bash
./scripts/build-and-test.sh
```

Если нужно без integration-тестов:

```bash
./scripts/build-and-test.sh --skip-integration
```

---

## Короткий сценарий для защиты

```text
1. ./scripts/start-postgres.sh
2. ./scripts/apply-migrations.sh
3. ./scripts/run-api.sh
4. ./scripts/run-web.sh
5. /system-overview
6. /administration
7. создать/выбрать состав группы
8. создать новую эпоху
9. /protocol-sessions
10. создать сессию
11. пройти commitment, reveal nonce, partial signatures
12. проверить подпись
13. сформировать отчёт
14. /protocol-session-history
15. /audit-log
16. показать PostgreSQL-запросы через psql
```
