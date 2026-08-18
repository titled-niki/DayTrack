@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title DayTrack Debug Build

dotnet restore DayTrack.sln
if errorlevel 1 goto :fail

dotnet build DayTrack.sln -c Debug --no-restore
if errorlevel 1 goto :fail

echo.
echo Build complete.
pause
exit /B 0

:fail
echo.
echo Build failed.
pause
exit /B 1
