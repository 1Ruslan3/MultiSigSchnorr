# rollback_package_26_modern_ui_theme

Откат пакета `MultiSigSchnorr_fix_package_26_modern_ui_theme`.

## Как применить

```bash
cd ~/Documents/GitHub/MultiSigSchnorr
unzip -o ~/Downloads/MultiSigSchnorr_rollback_package_26_modern_ui_theme.zip -d .
python3 scripts/rollback_modern_ui_theme.py
dotnet build
```

После этого перезапусти Web:

```bash
./scripts/run-web.sh
```

И в браузере сделай hard refresh:

```text
Cmd + Shift + R
```

## Что удаляется

```text
src/MultiSigSchnorr.Web/wwwroot/css/multisig-theme.css
src/MultiSigSchnorr.Web/wwwroot/js/multisig-theme.js
src/MultiSigSchnorr.Web/Components/Shared/ThemeToggle.razor
```

Также из `App.razor`, `_Imports.razor` и `MainLayout.razor` удаляются подключения темы и `<ThemeToggle />`.

Перед изменением файлов скрипт создаёт backup с суффиксом:

```text
.rollback-bak-YYYYMMDDHHMMSS
```
