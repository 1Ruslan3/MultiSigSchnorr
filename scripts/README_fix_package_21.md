# fix_package_21_protocol_session_url_state

Пакет исправляет потерю состояния страницы `/protocol-sessions` после перехода на `/protocol-math`.

## Что делает patch-скрипт

1. Добавляет в `ProtocolSessions.razor` поддержку query-параметра:

```text
/protocol-sessions?sessionId=<id>
```

2. При открытии страницы с `sessionId` загружает состояние сессии с backend:

```csharp
ApiClient.GetSessionStateAsync(sessionId)
```

3. После создания новой сессии пытается обновить URL:

```text
/protocol-sessions?sessionId=<created-session-id>
```

4. Исправляет кнопку возврата на Math-странице:

```text
/protocol-math/{sessionId}
        ↓
/protocol-sessions?sessionId={sessionId}
```

## Как применить

Из корня проекта:

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
python3 scripts/apply_protocol_session_url_state.py
dotnet build
```

## Что проверить

1. Открыть `/protocol-sessions`.
2. Создать сессию.
3. Выполнить один или несколько этапов.
4. Перейти на `/protocol-math/{sessionId}`.
5. Нажать "Вернуться к протоколу".
6. Страница должна открыться как:

```text
/protocol-sessions?sessionId=<id>
```

и восстановить состояние сессии.

## Если скрипт выдал warning

Он не стал грубо ломать Razor-файл. В таком случае пришли текущий файл:

```text
src/MultiSigSchnorr.Web/Components/Pages/ProtocolSessions.razor
```

и можно будет сделать точную полную замену.
