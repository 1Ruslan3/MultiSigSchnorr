# fix_package_22_protocol_session_restore_after_render

Этот пакет исправляет ситуацию, когда `/protocol-sessions` теряет состояние после перехода на `/protocol-math`.

Предыдущий фикс мог не сработать, если `ProtocolSessions.razor` сначала загружал `sessionId`,
а потом собственный `OnInitializedAsync` заново сбрасывал состояние страницы.

Новый patch-скрипт восстанавливает сессию после первичного render:

```csharp
OnAfterRenderAsync(firstRender)
```

То есть даже если начальная инициализация компонента что-то обнулила, после render страница читает:

```text
/protocol-sessions?sessionId=<id>
```

и заново загружает состояние с backend.

## Как применить

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
unzip -o ~/Downloads/MultiSigSchnorr_fix_package_22_protocol_session_restore_after_render.zip -d .
python3 scripts/apply_protocol_session_restore_after_render.py
dotnet build
```

## Что проверить

1. Открыть `/protocol-sessions`.
2. Создать сессию.
3. После создания URL должен стать:
   `/protocol-sessions?sessionId=<id>`.
4. Выполнить несколько действий протокола.
5. Перейти на `/protocol-math/<id>`.
6. Вернуться назад.
7. Страница должна восстановить состояние сессии.

## Если URL после создания сессии не меняется

Значит скрипт не нашёл место создания сессии в `ProtocolSessions.razor`.

Нужно вручную добавить после присваивания `_sessionState`:

```csharp
NavigateToProtocolSession(_sessionState.SessionId);
```

## Если сборка упала

Скрипт делает backup:

```text
ProtocolSessions.razor.bak-YYYYMMDDHHMMSS
ProtocolMath.razor.bak-YYYYMMDDHHMMSS
```

Можно откатиться или прислать текущий `ProtocolSessions.razor` для точной правки.
