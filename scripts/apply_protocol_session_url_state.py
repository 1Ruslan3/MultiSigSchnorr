#!/usr/bin/env python3
from __future__ import annotations

from pathlib import Path
import re
import sys
from datetime import datetime


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


def ensure_query_property_and_loader(text: str) -> str:
    if "SessionIdFromQuery" in text:
        return text

    code_match = re.search(r"@code\s*\{", text)
    if not code_match:
        raise RuntimeError("Не найден блок @code { ... } в ProtocolSessions.razor")

    insert_pos = code_match.end()

    block = r'''

    [SupplyParameterFromQuery(Name = "sessionId")]
    public Guid? SessionIdFromQuery { get; set; }

    private Guid? _loadedSessionIdFromQuery;

    protected override async Task OnParametersSetAsync()
    {
        if (SessionIdFromQuery is null || SessionIdFromQuery.Value == Guid.Empty)
            return;

        if (_loadedSessionIdFromQuery == SessionIdFromQuery.Value)
            return;

        _loadedSessionIdFromQuery = SessionIdFromQuery.Value;
        await LoadSessionFromUrlAsync(SessionIdFromQuery.Value);
    }

    private async Task LoadSessionFromUrlAsync(Guid sessionId)
    {
        try
        {
            _error = null;
            _isBusy = true;

            _sessionState = await ApiClient.GetSessionStateAsync(sessionId);
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _isBusy = false;
        }
    }
'''

    return text[:insert_pos] + block + text[insert_pos:]


def ensure_update_url_after_session_creation(text: str) -> str:
    if "EnsureProtocolSessionUrl(" in text:
        return text

    helper = r'''

    private void EnsureProtocolSessionUrl(Guid sessionId)
    {
        var targetUrl = $"/protocol-sessions?sessionId={sessionId}";

        if (!Navigation.Uri.EndsWith(targetUrl, StringComparison.OrdinalIgnoreCase))
        {
            Navigation.NavigateTo(targetUrl, replace: true);
        }
    }
'''

    last_brace = text.rfind("}")
    if last_brace == -1:
        raise RuntimeError("Не удалось найти закрывающую скобку @code блока.")

    text = text[:last_brace] + helper + text[last_brace:]

    patterns = [
        r"(_sessionState\s*=\s*await\s+ApiClient\.CreateProtocolSessionAsync\([^;]+;\s*)",
        r"(_sessionState\s*=\s*await\s+ApiClient\.CreateSessionAsync\([^;]+;\s*)",
        r"(_sessionState\s*=\s*createdSession;\s*)",
        r"(_sessionState\s*=\s*session;\s*)",
    ]

    inserted = False
    for pattern in patterns:
        def repl(match: re.Match[str]) -> str:
            nonlocal inserted
            inserted = True
            return match.group(1) + "\n            EnsureProtocolSessionUrl(_sessionState.SessionId);\n"

        text_new = re.sub(pattern, repl, text, count=1, flags=re.DOTALL)
        if inserted:
            text = text_new
            break

    if not inserted:
        print("warning: не удалось автоматически найти место создания сессии.")
        print("warning: добавь вручную после присваивания _sessionState:")
        print("warning: EnsureProtocolSessionUrl(_sessionState.SessionId);")

    return text


def ensure_math_link(text: str) -> str:
    if "protocol-math" in text:
        return text

    marker_patterns = [
        r"(<button[^>]*Verify[^<]*</button>)",
        r"(<button[^>]*Провер[^<]*</button>)",
        r"(<a[^>]*report[^>]*>[^<]*</a>)",
    ]

    link = r'''
                    <a class="btn btn-outline-primary"
                       href="@($"/protocol-math/{_sessionState.SessionId}")">
                        Математический разбор
                    </a>
'''

    for pattern in marker_patterns:
        match = re.search(pattern, text, flags=re.IGNORECASE | re.DOTALL)
        if match:
            return text[:match.end()] + "\n" + link + text[match.end():]

    print("warning: не удалось автоматически вставить ссылку на protocol-math.")
    print("warning: добавь вручную рядом с действиями текущей сессии:")
    print(link)
    return text


def patch_protocol_sessions() -> None:
    if not PROTOCOL_SESSIONS.exists():
        raise FileNotFoundError(PROTOCOL_SESSIONS)

    backup(PROTOCOL_SESSIONS)
    text = PROTOCOL_SESSIONS.read_text(encoding="utf-8")

    required_tokens = ["_sessionState", "ApiClient", "_error", "_isBusy"]
    missing = [token for token in required_tokens if token not in text]
    if missing:
        raise RuntimeError(
            "ProtocolSessions.razor имеет неожиданную структуру. "
            f"Не найдены токены: {', '.join(missing)}. "
            "Нужна ручная правка текущего файла."
        )

    text = ensure_navigation_inject(text)
    text = ensure_query_property_and_loader(text)
    text = ensure_update_url_after_session_creation(text)
    text = ensure_math_link(text)

    PROTOCOL_SESSIONS.write_text(text, encoding="utf-8")
    print(f"patched: {PROTOCOL_SESSIONS}")


def patch_protocol_math() -> None:
    if not PROTOCOL_MATH.exists():
        print(f"skip: {PROTOCOL_MATH} not found")
        return

    backup(PROTOCOL_MATH)
    text = PROTOCOL_MATH.read_text(encoding="utf-8")

    old_variants = [
        'href="protocol-sessions"',
        'href="/protocol-sessions"',
        'href="@($"/protocol-sessions")"',
    ]

    new_link = 'href="@($"/protocol-sessions?sessionId={_trace.SessionId}")"'

    replaced = False
    for old in old_variants:
        if old in text:
            text = text.replace(old, new_link)
            replaced = True

    if "protocol-sessions?sessionId={_trace.SessionId}" in text:
        replaced = True

    if not replaced:
        print("warning: не удалось автоматически найти кнопку возврата в ProtocolMath.razor.")
        print("warning: замени ссылку возврата вручную на:")
        print(new_link)

    PROTOCOL_MATH.write_text(text, encoding="utf-8")
    print(f"patched: {PROTOCOL_MATH}")


def main() -> int:
    try:
        patch_protocol_sessions()
        patch_protocol_math()
        print()
        print("Готово. Теперь выполните:")
        print("dotnet build")
        return 0
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
