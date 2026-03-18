@echo off
echo Building CATUI Toolbelt Fix Mod...
dotnet build CATUI_toolbelt_fix.csproj -c Release
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b %errorlevel%
)
echo Build successful!
echo DLL created: CATUI_toolbelt_fix.dll
pause

