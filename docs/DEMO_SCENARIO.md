# Демонстрационный сценарий MultiSigSchnorr

## Назначение сценария

Данный документ описывает последовательность действий для демонстрации программной системы `MultiSigSchnorr`.

Сценарий предназначен для защиты выпускной квалификационной работы и показывает:

- запуск инфраструктуры;
- работу API;
- работу Web-интерфейса;
- создание коллективной подписи;
- прохождение этапов протокола;
- проверку итоговой подписи;
- сохранение публичного состояния в PostgreSQL;
- аудит действий;
- просмотр отчётных данных.

---

## Предварительные требования

На компьютере должны быть установлены:

- .NET SDK 10;
- Docker Desktop;
- PowerShell;
- браузер для открытия Web-интерфейса.

PostgreSQL запускается через Docker Compose. Устанавливать PostgreSQL напрямую в Windows не требуется.

---

## 1. Переход в каталог проекта

Открыть PowerShell и перейти в каталог проекта:

```powershell
cd C:\Users\user\Desktop\diplom\MultiSigSchnorr
```

---

## 2. Запуск PostgreSQL

Запустить PostgreSQL через Docker Compose:

```powershell
docker compose -f .\deploy\docker-compose.postgres.yml up -d
```

Проверить, что контейнер запущен:

```powershell
docker ps
```

Ожидаемый контейнер:

```text
multisig-postgres
```

При необходимости посмотреть логи:

```powershell
docker logs multisig-postgres
```

---

## 3. Проверка базы данных

Подключиться к PostgreSQL внутри контейнера:

```powershell
docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
```

Проверить список таблиц:

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

Выйти из `psql`:

```sql
\q
```

---

## 4. Запуск API

В отдельном окне PowerShell из корня проекта выполнить:

```powershell
dotnet run --project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj
```

Ожидаемый адрес API:

```text
http://localhost:5227
```

Проверить в браузере:

```text
http://localhost:5227
```

Ожидается JSON-ответ с информацией о сервисе, доступных маршрутах и состоянии seed-данных.

---

## 5. Запуск Web-интерфейса

В новом окне PowerShell из корня проекта выполнить:

```powershell
dotnet run --project .\src\MultiSigSchnorr.Web\MultiSigSchnorr.Web.csproj
```

Открыть Web-интерфейс:

```text
http://localhost:5080
```

---

## 6. Проверка System Overview

Открыть страницу:

```text
http://localhost:5080/system-overview
```

На странице должны отображаться:

- активная эпоха;
- seed-участники;
- публичные ключи участников;
- PostgreSQL diagnostics;
- количество записей в таблицах;
- количество применённых миграций;
- последняя миграция;
- состояние подключения к БД.

Особое внимание при демонстрации:

```text
Connection: Connected
Storage Provider: PostgreSQL + EF Core
```

Этот блок показывает, что система использует PostgreSQL, а не только in-memory хранение.

---

## 7. Создание протокольной сессии

Открыть страницу:

```text
http://localhost:5080/protocol-sessions
```

На странице отображаются:

- активная эпоха;
- список seed-участников;
- поле сообщения;
- выбор режима защиты;
- кнопка создания протокольной сессии.

В поле сообщения можно оставить значение:

```text
demo-protocol-session
```

В режиме защиты выбрать:

```text
RandomizedScalarProcessing
```

Нажать:

```text
Создать протокольную сессию
```

После создания должна появиться карточка сессии:

- Session ID;
- Epoch ID;
- Protection Mode;
- Message Digest;
- Aggregate Public Key;
- список участников;
- состояние этапов протокола.

---

## 8. Этап 1: Publish Commitment

Для каждого участника нажать кнопку:

```text
Publish Commitment
```

После публикации commitment у каждого участника должен появиться признак успешного выполнения commitment-этапа.

После публикации commitment всеми участниками флаг:

```text
All Commitments Published
```

должен стать `true`.

---

## 9. Этап 2: Reveal Nonce

После завершения commitment-этапа для каждого участника нажать:

```text
Reveal Nonce
```

После раскрытия nonce у каждого участника должен появиться публичный nonce point.

После раскрытия nonce всеми участниками флаг:

```text
All Nonces Revealed
```

должен стать `true`.

Также должна появиться информация:

- Aggregate Nonce Point;
- Challenge.

---

## 10. Этап 3: Submit Partial Signature

После раскрытия nonce для каждого участника нажать:

```text
Submit Partial Signature
```

После отправки частичной подписи у каждого участника должен появиться scalar частичной подписи.

После отправки частичных подписей всеми участниками:

```text
All Partial Signatures Submitted
```

должен стать `true`.

Статус сессии должен стать:

```text
Completed
```

Также должны появиться:

- Aggregate Signature Nonce Point;
- Aggregate Signature Scalar.

---

## 11. Проверка итоговой подписи

После завершения сессии нажать:

```text
Проверить итоговую подпись
```

Ожидаемый результат:

```text
Aggregate signature is valid.
```

Это демонстрирует, что итоговая коллективная подпись корректно проверяется.

---

## 12. Формирование отчёта

Нажать:

```text
Сформировать отчёт
```

На странице появится блок отчёта с данными:

- Created UTC;
- Completed UTC;
- Status;
- Protection Mode;
- Participants Count;
- Aggregate Signature Scalar;
- флаги этапов протокола.

Также можно скачать отчёты:

```text
Скачать JSON
Скачать TXT
```

---

## 13. Проверка истории сессий

Открыть страницу:

```text
http://localhost:5080/protocol-session-history
```

На странице должна отображаться созданная протокольная сессия.

Для каждой записи отображаются:

- Session ID;
- Epoch;
- Status;
- Protection Mode;
- Created UTC;
- Completed UTC;
- количество участников;
- состояние этапов.

---

## 14. Проверка Audit Log

Открыть страницу:

```text
http://localhost:5080/audit-log
```

В журнале должны быть записи о ключевых действиях:

- создание протокольной сессии;
- отзыв участника, если выполнялся;
- переход эпохи, если выполнялся.

На странице доступны фильтры:

- поиск по тексту;
- фильтр по Action Type;
- фильтр по Entity Type;
- фильтр по Entity ID.

Пример поиска:

```text
RandomizedScalarProcessing
```

Так можно показать, что режим защиты фиксируется в аудите.

---

## 15. Проверка PostgreSQL-проекции сессии

Подключиться к PostgreSQL:

```powershell
docker exec -it multisig-postgres psql -U multisig_user -d multisig_schnorr
```

Проверить таблицу сессий:

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

Ожидается, что завершённая сессия сохранена в PostgreSQL.

---

## 16. Проверка сохранения после перезапуска API

1. Остановить API через `Ctrl + C`.
2. Запустить API заново:

```powershell
dotnet run --project .\src\MultiSigSchnorr.Api\MultiSigSchnorr.Api.csproj
```

3. Открыть:

```text
http://localhost:5080/protocol-session-history
```

Созданная ранее сессия должна остаться в истории.

4. Открыть сессию.

Ожидаемый результат:

- сессия доступна для просмотра;
- отчёт доступен;
- публичные артефакты отображаются;
- действия продолжения протокола недоступны, если runtime-состояние отсутствует.

Это демонстрирует разделение:

```text
runtime-состояние -> in-memory
публичная история -> PostgreSQL
```

---

## 17. Демонстрация администрирования

Открыть страницу:

```text
http://localhost:5080/administration
```

На странице можно показать:

- активную эпоху;
- список участников;
- статус участников;
- переход к следующей эпохе;
- отзыв участника.

После выполнения административного действия следует открыть:

```text
http://localhost:5080/audit-log
```

и показать, что действие попало в журнал аудита.

---

## 18. Демонстрация бенчмарков

Запустить бенчмарки:

```powershell
dotnet run --project .\tests\MultiSigSchnorr.Benchmarks\MultiSigSchnorr.Benchmarks.csproj -c Release
```

Бенчмарки сравнивают:

- генерацию открытого ключа в режиме `Baseline`;
- генерацию открытого ключа в режиме `RandomizedScalarProcessing`;
- вычисление частичной подписи в обоих режимах;
- полный протокольный цикл в обоих режимах.

Результаты позволяют показать накладные расходы защитного режима.

---

## 19. Основной тезис демонстрации

В ходе демонстрации важно подчеркнуть:

1. Система реализует полный цикл коллективной подписи Шнорра.
2. В протоколе участвуют несколько подписантов.
3. Итоговая подпись формируется из частичных подписей.
4. Подпись проверяется как единый агрегированный результат.
5. Реализован режим рандомизированной обработки секретного скаляра.
6. Публичное состояние протокола сохраняется в PostgreSQL.
7. Секретные материалы не сохраняются в базе данных.
8. Действия фиксируются в журнале аудита.
9. Система содержит API, Web-интерфейс, тесты и бенчмарки.

---

## 20. Краткая последовательность для защиты

Минимальный сценарий, если времени мало:

```text
1. Показать docker ps с PostgreSQL.
2. Открыть /system-overview.
3. Показать PostgreSQL diagnostics.
4. Создать сессию на /protocol-sessions.
5. Выбрать RandomizedScalarProcessing.
6. Пройти commitment.
7. Пройти reveal nonce.
8. Отправить partial signatures.
9. Показать статус Completed.
10. Проверить подпись.
11. Скачать отчёт.
12. Открыть /protocol-session-history.
13. Открыть /audit-log.
14. Показать записи в PostgreSQL через psql.
```
