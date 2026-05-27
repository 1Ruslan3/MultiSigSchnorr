#!/usr/bin/env python3
from __future__ import annotations

from datetime import datetime
from pathlib import Path
import re
import sys


PROJECT_ROOT = Path.cwd()

PROTOCOL_SESSIONS = PROJECT_ROOT / "src/MultiSigSchnorr.Web/Components/Pages/ProtocolSessions.razor"
PROTOCOL_MATH = PROJECT_ROOT / "src/MultiSigSchnorr.Web/Components/Pages/ProtocolMath.razor"


def backup(path: Path) -> None:
    stamp = datetime.now().strftime("%Y%m%d%H%M%S")
    backup_path = path.with_suffix(path.suffix + f".bak-{stamp}")
    backup_path.write_text(path.read_text(encoding="utf-8"), encoding="utf-8")
    print(f"backup: {backup_path}")


def ensure_navigation_inject(text: str) -> str:
    if "@inject NavigationManager Navigation" in text:
        return text

    lines = text.splitlines()
    insert_at = 0

    for index, line in enumerate(lines):
        if line.startswith("@inject "):
            insert_at = index + 1

    if insert_at == 0:
        for index, line in enumerate(lines):
            if line.startswith("@page ") or line.startswith("@using ") or line.startswith("@rendermode "):
                insert_at = index + 1

    lines.insert(insert_at, "@inject NavigationManager Navigation")
    return "\n".join(lines) + "\n"


def ensure_after_render_restore(text: str) -> str:
    if "RestoreProtocolSessionFromUrlAfterRenderAsync" in text:
        return text

    if "protected override async Task OnAfterRenderAsync" in text or "override Task OnAfterRenderAsync" in text:
        print("warning: в ProtocolSessions.razor уже есть OnAfterRenderAsync.")
        print("warning: автоматическая вставка пропущена, чтобы не создать duplicate method.")
        print("warning: пришли файл ProtocolSessions.razor, если после этого состояние всё ещё слетает.")
        return text

    code_match = re.search(r"@code\s*\{", text)
    if not code_match:
        raise RuntimeError("Не найден блок @code { ... } в ProtocolSessions.razor")

    insert_pos = code_match.end()

    block = r'''

    private bool _protocolSessionRestoredFromUrlAfterRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _protocolSessionRestoredFromUrlAfterRender)
            return;

        _protocolSessionRestoredFromUrlAfterRender = true;

        await RestoreProtocolSessionFromUrlAfterRenderAsync();
    }

    private async Task RestoreProtocolSessionFromUrlAfterRenderAsync()
    {
        var sessionId = TryGetProtocolSessionIdFromCurrentUrl();

        if (sessionId is null || sessionId.Value == Guid.Empty)
            return;

        try
        {
            _error = null;
            _isBusy = true;

            _sessionState = await ApiClient.GetSessionStateAsync(sessionId.Value);

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private Guid? TryGetProtocolSessionIdFromCurrentUrl()
    {
        var uri = new Uri(Navigation.Uri);
        var query = uri.Query;

        if (string.IsNullOrWhiteSpace(query))
            return null;

        if (query.StartsWith("?"))
            query = query[1..];

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);

            if (pair.Length != 2)
                continue;

            var key = Uri.UnescapeDataString(pair[0]);

            if (!string.Equals(key, "sessionId", StringComparison.OrdinalIgnoreCase))
                continue;

            var value = Uri.UnescapeDataString(pair[1]);

            if (Guid.TryParse(value, out var sessionId))
                return sessionId;
        }

        return null;
    }

    private void NavigateToProtocolSession(Guid sessionId)
    {
        var targetUrl = $"/protocol-sessions?sessionId={sessionId}";

        if (!Navigation.Uri.EndsWith(targetUrl, StringComparison.OrdinalIgnoreCase))
        {
            Navigation.NavigateTo(targetUrl, replace: true);
        }
    }
'''

    return text[:insert_pos] + block + text[insert_pos:]


def ensure_create_session_navigation_call(text: str) -> str:
    # If previous patch added EnsureProtocolSessionUrl, keep it, but also add robust NavigateToProtocolSession
    # after likely session creation assignments if not already present.
    if "NavigateToProtocolSession(_sessionState.SessionId)" in text:
        return text

    patterns = [
        r"(_sessionState\s*=\s*await\s+ApiClient\.CreateProtocolSessionAsync\([^;]+;\s*)",
        r"(_sessionState\s*=\s*await\s+ApiClient\.CreateSessionAsync\([^;]+;\s*)",
        r"(_sessionState\s*=\s*createdSession;\s*)",
        r"(_sessionState\s*=\s*session;\s*)",
    ]

    for pattern in patterns:
        def repl(match: re.Match[str]) -> str:
            return match.group(1) + "\n            NavigateToProtocolSession(_sessionState.SessionId);\n"

        text_new, count = re.subn(pattern, repl, text, count=1, flags=re.DOTALL)
        if count > 0:
            print("patched: добавлен NavigateToProtocolSession после создания сессии")
            return text_new

    print("warning: не удалось найти место создания сессии для NavigateToProtocolSession.")
    print("warning: добавь вручную после присваивания _sessionState:")
    print("warning: NavigateToProtocolSession(_sessionState.SessionId);")
    return text


def patch_protocol_math_return_link(text: str) -> str:
    # Replace common return links to protocol-sessions with query-preserving link.
    target = 'href="@($"/protocol-sessions?sessionId={_trace.SessionId}")"'

    if 'protocol-sessions?sessionId={_trace.SessionId}' in text:
        return text

    replacements = [
        'href="protocol-sessions"',
        'href="/protocol-sessions"',
        'href="@($"/protocol-sessions")"',
        'href="@("protocol-sessions")"',
    ]

    replaced = False
    for old in replacements:
        if old in text:
            text = text.replace(old, target)
            replaced = True

    if not replaced:
        print("warning: не удалось автоматически заменить ссылку возврата в ProtocolMath.razor.")
        print("warning: ссылка должна быть такой:")
        print(target)

    return text


def patch_protocol_sessions() -> None:
    if not PROTOCOL_SESSIONS.exists():
        raise FileNotFoundError(PROTOCOL_SESSIONS)

    backup(PROTOCOL_SESSIONS)
    text = PROTOCOL_SESSIONS.read_text(encoding="utf-8")

    for token in ["_sessionState", "ApiClient", "_error", "_isBusy"]:
        if token not in text:
            raise RuntimeError(f"ProtocolSessions.razor имеет неожиданную структуру: не найден {token}")

    text = ensure_navigation_inject(text)
    text = ensure_after_render_restore(text)
    text = ensure_create_session_navigation_call(text)

    PROTOCOL_SESSIONS.write_text(text, encoding="utf-8")
    print(f"patched: {PROTOCOL_SESSIONS}")


def patch_protocol_math() -> None:
    if not PROTOCOL_MATH.exists():
        print(f"skip: {PROTOCOL_MATH} not found")
        return

    backup(PROTOCOL_MATH)
    text = PROTOCOL_MATH.read_text(encoding="utf-8")
    text = patch_protocol_math_return_link(text)
    PROTOCOL_MATH.write_text(text, encoding="utf-8")
    print(f"patched: {PROTOCOL_MATH}")


def main() -> int:
    try:
        patch_protocol_sessions()
        patch_protocol_math()
        print()
        print("Готово. Теперь выполните:")
        print("dotnet build")
        print()
        print("Проверка:")
        print("1. Создать сессию.")
        print("2. Убедиться, что URL стал /protocol-sessions?sessionId=<id>.")
        print("3. Перейти на Math.")
        print("4. Вернуться назад.")
        print("5. Состояние должно восстановиться после первого render.")
        return 0
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
