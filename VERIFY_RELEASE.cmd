@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title DayTrack v1.0.0 Release Verification

echo.
echo ========== DAYTRACK v1.0.0 RELEASE ==========
echo.

if exist "dist\DayTrack-v1.0.0-Portable-win-x64.zip" (
    echo [ OK ] Portable ZIP
) else (
    echo [FAIL] Portable ZIP missing
)

if exist "dist\DayTrack-Setup-v1.0.0-win-x64.exe" (
    echo [ OK ] Setup EXE
) else (
    echo [FAIL] Setup EXE missing
)

if exist "dist\SHA256SUMS.txt" (
    echo [ OK ] SHA256SUMS.txt
) else (
    echo [FAIL] SHA256SUMS.txt missing
)

echo.
pause
