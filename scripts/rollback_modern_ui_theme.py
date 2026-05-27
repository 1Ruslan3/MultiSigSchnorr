#!/usr/bin/env python3
from __future__ import annotations

from datetime import datetime
from pathlib import Path
import re
import sys


ROOT = Path.cwd()

FILES_TO_DELETE = [
    ROOT / "src/MultiSigSchnorr.Web/wwwroot/css/multisig-theme.css",
    ROOT / "src/MultiSigSchnorr.Web/wwwroot/js/multisig-theme.js",
    ROOT / "src/MultiSigSchnorr.Web/Components/Shared/ThemeToggle.razor",
]

APP_RAZOR_CANDIDATES = [
    ROOT / "src/MultiSigSchnorr.Web/Components/App.razor",
    ROOT / "src/MultiSigSchnorr.Web/App.razor",
    ROOT / "src/MultiSigSchnorr.Web/Pages/_Host.cshtml",
]

IMPORTS = ROOT / "src/MultiSigSchnorr.Web/Components/_Imports.razor"
MAIN_LAYOUT = ROOT / "src/MultiSigSchnorr.Web/Components/Layout/MainLayout.razor"


def backup(path: Path) -> None:
    if not path.exists():
        return

    stamp = datetime.now().strftime("%Y%m%d%H%M%S")
    backup_path = path.with_suffix(path.suffix + f".rollback-bak-{stamp}")
    backup_path.write_text(path.read_text(encoding="utf-8"), encoding="utf-8")
    print(f"backup: {backup_path}")


def remove_file(path: Path) -> None:
    if path.exists():
        path.unlink()
        print(f"deleted: {path}")
    else:
        print(f"skip missing: {path}")


def clean_app_shell() -> None:
    for path in APP_RAZOR_CANDIDATES:
        if not path.exists():
            continue

        text = path.read_text(encoding="utf-8")

        if "multisig-theme.css" not in text and "multisig-theme.js" not in text:
            continue

        backup(path)

        text = re.sub(
            r'\s*<link\s+rel="stylesheet"\s+href="css/multisig-theme\.css"\s*/>\s*',
            "\n",
            text)

        text = re.sub(
            r'\s*<script\s+src="js/multisig-theme\.js">\s*</script>\s*',
            "\n",
            text)

        path.write_text(text, encoding="utf-8")
        print(f"cleaned: {path}")


def clean_imports() -> None:
    if not IMPORTS.exists():
        print(f"skip missing: {IMPORTS}")
        return

    text = IMPORTS.read_text(encoding="utf-8")
    line = "@using MultiSigSchnorr.Web.Components.Shared"

    if line not in text:
        print(f"skip unchanged: {IMPORTS}")
        return

    backup(IMPORTS)

    lines = [x for x in text.splitlines() if x.strip() != line]
    IMPORTS.write_text("\n".join(lines).rstrip() + "\n", encoding="utf-8")
    print(f"cleaned: {IMPORTS}")


def clean_main_layout() -> None:
    if not MAIN_LAYOUT.exists():
        print(f"skip missing: {MAIN_LAYOUT}")
        return

    text = MAIN_LAYOUT.read_text(encoding="utf-8")

    if "<ThemeToggle" not in text and "theme-toggle-wrapper" not in text:
        print(f"skip unchanged: {MAIN_LAYOUT}")
        return

    backup(MAIN_LAYOUT)

    text = re.sub(
        r'\s*<ThemeToggle\s*/>\s*',
        "\n",
        text)

    text = re.sub(
        r'\s*<div\s+class="theme-toggle-wrapper">\s*</div>\s*',
        "\n",
        text,
        flags=re.DOTALL)

    text = text.replace(
        '<div class="theme-toggle-wrapper">\n\n@Body',
        '@Body')

    text = text.replace(
        '<div class="theme-toggle-wrapper">\n@Body',
        '@Body')

    text = re.sub(
        r'<div\s+class="theme-toggle-wrapper">\s*@Body\s*</div>',
        '@Body',
        text,
        flags=re.DOTALL)

    MAIN_LAYOUT.write_text(text, encoding="utf-8")
    print(f"cleaned: {MAIN_LAYOUT}")


def main() -> int:
    try:
        for path in FILES_TO_DELETE:
            remove_file(path)

        clean_app_shell()
        clean_imports()
        clean_main_layout()

        print()
        print("Откат выполнен.")
        print("Теперь выполни:")
        print("dotnet build")
        print()
        print("Если браузер всё ещё показывает старые стили:")
        print("Cmd + Shift + R")
        print("или очисти site data для localhost:5080.")
        return 0
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
