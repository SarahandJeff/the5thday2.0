
cd \
cd 7d2d
cd custom
cd 5th_day_reloaded
cd mods
pause

setlocal enabledelayedexpansion
set "found=0"
if exist combined_localization.txt del /f /q combined_localization.txt
for /r %%f in (localization.txt) do (
  if "!found!"=="0" (
    >combined_localization.txt echo Combined localizations
    set "found=1"
  )

  type "%%f" >> combined_localization.txt
  echo. "%%f ">> combined_localization.txt
)

if "%found%"=="0" (
  echo No localization.txt files found. No combined_localization.txt created.
)
endlocal