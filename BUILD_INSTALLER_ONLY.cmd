@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title DayTrack v1.0.0 Installer Builder

if not exist "release\DayTrack-win-x64\DayTrack.exe" (
    echo ERROR: Published DayTrack.exe does not exist.
    echo Run BUILD_PUBLIC_RELEASE.cmd first.
    pause
    exit /B 1
)

set "ISCC="
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"

if not defined ISCC (
    echo ERROR: Inno Setup 6 was not found.
    pause
    exit /B 1
)

if not exist "dist" mkdir "dist"

"%ISCC%" "installer\DayTrack.iss"
if errorlevel 1 (
    echo Installer build failed.
    pause
    exit /B 1
)

echo.
echo Installer ready:
dir /B "dist\DayTrack-Setup-v1.0.0-win-x64.exe"
echo.
pause
