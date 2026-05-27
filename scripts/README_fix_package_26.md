# fix_package_26_modern_ui_theme

Пакет добавляет современный слой оформления и переключатель темы Light/Dark.

## Как применить

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
unzip -o ~/Downloads/MultiSigSchnorr_fix_package_26_modern_ui_theme.zip -d .
python3 scripts/apply_modern_ui_theme.py
dotnet build
```

Перезапусти Web:

```bash
./scripts/run-web.sh
```

## Что добавляется

```text
src/MultiSigSchnorr.Web/wwwroot/css/multisig-theme.css
src/MultiSigSchnorr.Web/wwwroot/js/multisig-theme.js
src/MultiSigSchnorr.Web/Components/Shared/ThemeToggle.razor
scripts/apply_modern_ui_theme.py
```

Скрипт подключает CSS/JS, добавляет using для Shared-компонентов и пытается вставить `<ThemeToggle />` в `MainLayout.razor`.

Если переключатель темы окажется не там, где нужно, перемести строку:

```razor
<ThemeToggle />
```

в верхнюю панель навигации.
