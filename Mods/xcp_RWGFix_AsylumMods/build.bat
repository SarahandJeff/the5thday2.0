@echo off
echo ========================================
echo Building Road Shape Fix Mod
echo ========================================
echo.

REM Set the path to MSBuild (adjust if needed for your system)
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

REM Check if MSBuild exists at the specified path
if not exist %MSBUILD% (
    echo MSBuild not found at %MSBUILD%
    echo Trying alternative paths...
    set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

if not exist %MSBUILD% (
    echo MSBuild not found. Please install Visual Studio or adjust the path in build.bat
    pause
    exit /b 1
)

echo Using MSBuild: %MSBUILD%
echo.

REM Build the project
%MSBUILD% RoadShapeFix.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build completed successfully!
    echo DLL location: RoadShapeFix.dll
    echo ========================================
) else (
    echo.
    echo ========================================
    echo Build failed! Check errors above.
    echo ========================================
)

echo.
pause

