#!/usr/bin/env python3
from pathlib import Path
from datetime import datetime

path = Path("src/MultiSigSchnorr.Web/Components/Pages/ProtocolMath.razor")

if not path.exists():
    raise SystemExit(f"Файл не найден: {path}")

stamp = datetime.now().strftime("%Y%m%d%H%M%S")
backup_path = path.with_suffix(path.suffix + f".bak-{stamp}")
backup_path.write_text(path.read_text(encoding="utf-8"), encoding="utf-8")

text = path.read_text(encoding="utf-8")

target = 'href="@($"/protocol-sessions?sessionId={_trace.SessionId}")"'

variants = [
    'href="protocol-sessions"',
    'href="/protocol-sessions"',
    'href="@($"/protocol-sessions")"',
    'href="@("protocol-sessions")"',
]

changed = False

if "protocol-sessions?sessionId={_trace.SessionId}" not in text:
    for variant in variants:
        if variant in text:
            text = text.replace(variant, target)
            changed = True

if changed:
    path.write_text(text, encoding="utf-8")
    print(f"patched: {path}")
else:
    print("Ссылка уже исправлена или не найдена.")
    print("Проверь вручную, что кнопка возврата в ProtocolMath.razor ведёт на:")
    print(target)

print(f"backup: {backup_path}")
