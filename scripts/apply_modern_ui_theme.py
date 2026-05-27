#!/usr/bin/env python3
from __future__ import annotations

from datetime import datetime
from pathlib import Path
import re
import sys


ROOT = Path.cwd()

APP_RAZOR_CANDIDATES = [
    ROOT / "src/MultiSigSchnorr.Web/Components/App.razor",
    ROOT / "src/MultiSigSchnorr.Web/App.razor",
    ROOT / "src/MultiSigSchnorr.Web/Pages/_Host.cshtml",
]

IMPORTS = ROOT / "src/MultiSigSchnorr.Web/Components/_Imports.razor"
MAIN_LAYOUT = ROOT / "src/MultiSigSchnorr.Web/Components/Layout/MainLayout.razor"


def backup(path: Path) -> None:
    stamp = datetime.now().strftime("%Y%m%d%H%M%S")
    backup_path = path.with_suffix(path.suffix + f".bak-{stamp}")
    backup_path.write_text(path.read_text(encoding="utf-8"), encoding="utf-8")
    print(f"backup: {backup_path}")


def patch_app_shell() -> None:
    app_file = next((x for x in APP_RAZOR_CANDIDATES if x.exists()), None)

    if app_file is None:
        print("warning: не найден App.razor/_Host.cshtml. Добавь вручную:")
        print('<link rel="stylesheet" href="css/multisig-theme.css" />')
        print('<script src="js/multisig-theme.js"></script>')
        return

    backup(app_file)
    text = app_file.read_text(encoding="utf-8")

    if "css/multisig-theme.css" not in text and "</head>" in text:
        text = text.replace("</head>", '    <link rel="stylesheet" href="css/multisig-theme.css" />\n</head>')

    if "js/multisig-theme.js" not in text and "</body>" in text:
        text = text.replace("</body>", '    <script src="js/multisig-theme.js"></script>\n</body>')

    app_file.write_text(text, encoding="utf-8")
    print(f"patched: {app_file}")


def patch_imports() -> None:
    if not IMPORTS.exists():
        print(f"warning: не найден {IMPORTS}")
        return

    backup(IMPORTS)
    text = IMPORTS.read_text(encoding="utf-8")
    line = "@using MultiSigSchnorr.Web.Components.Shared"

    if line not in text:
        text = text.rstrip() + "\n" + line + "\n"

    IMPORTS.write_text(text, encoding="utf-8")
    print(f"patched: {IMPORTS}")


def patch_main_layout() -> None:
    if not MAIN_LAYOUT.exists():
        print(f"warning: не найден {MAIN_LAYOUT}")
        print("Добавь вручную в верхнюю панель: <ThemeToggle />")
        return

    backup(MAIN_LAYOUT)
    text = MAIN_LAYOUT.read_text(encoding="utf-8")

    if "<ThemeToggle" in text:
        print(f"skip: ThemeToggle уже есть в {MAIN_LAYOUT}")
        return

    insertion = "\n        <ThemeToggle />\n"

    patterns = [
        r"(<div[^>]*class=\"[^\"]*(?:top-row|topbar|app-header|navbar|nav-actions)[^\"]*\"[^>]*>)",
        r"(<header[^>]*>)",
    ]

    for pattern in patterns:
        match = re.search(pattern, text, flags=re.IGNORECASE)
        if match:
            text = text[:match.end()] + insertion + text[match.end():]
            MAIN_LAYOUT.write_text(text, encoding="utf-8")
            print(f"patched: {MAIN_LAYOUT}")
            return

    if "@Body" in text:
        text = text.replace("@Body", "<div class=\"theme-toggle-wrapper\"><ThemeToggle /></div>\n@Body", 1)
        MAIN_LAYOUT.write_text(text, encoding="utf-8")
        print(f"patched with fallback: {MAIN_LAYOUT}")
        return

    print("warning: не удалось автоматически вставить ThemeToggle.")
    print("Добавь вручную в MainLayout.razor рядом с навигацией: <ThemeToggle />")


def main() -> int:
    try:
        patch_app_shell()
        patch_imports()
        patch_main_layout()
        print()
        print("Готово. Теперь выполни:")
        print("dotnet build")
        print()
        print("Если ThemeToggle попал не туда, перемести строку <ThemeToggle /> в верхнюю панель MainLayout.razor.")
        return 0
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
