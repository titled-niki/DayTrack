@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title DayTrack v1.0.0 Public Release Builder

set "VERSION=1.0.0"
set "RID=win-x64"
set "PROJECT=src\DayTrack.App\DayTrack.App.csproj"
set "RELEASE_DIR=%CD%\release\DayTrack-win-x64"
set "DIST_DIR=%CD%\dist"
set "PORTABLE_ZIP=%DIST_DIR%\DayTrack-v%VERSION%-Portable-%RID%.zip"

echo.
echo ==========================================================
echo          DAYTRACK v%VERSION% PUBLIC RELEASE BUILD
echo ==========================================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet SDK was not found.
    goto :fail
)

echo [1/8] Cleaning old release files...
if exist "%CD%\release" rmdir /S /Q "%CD%\release"
if exist "%DIST_DIR%" rmdir /S /Q "%DIST_DIR%"
mkdir "%RELEASE_DIR%"
mkdir "%DIST_DIR%"

echo [2/8] Restoring packages for %RID%...
dotnet restore "%PROJECT%" -r %RID%
if errorlevel 1 goto :fail

echo [3/8] Release compile check...
dotnet build "%PROJECT%" -c Release -r %RID% --no-restore
if errorlevel 1 goto :fail

echo [4/8] Publishing self-contained Windows x64 build...
dotnet publish "%PROJECT%" ^
  -c Release ^
  -r %RID% ^
  --self-contained true ^
  --no-restore ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=None ^
  -p:DebugSymbols=false ^
  -p:PublishReadyToRun=false ^
  -o "%RELEASE_DIR%"
if errorlevel 1 goto :fail

if not exist "%RELEASE_DIR%\DayTrack.exe" (
    echo ERROR: DayTrack.exe was not created.
    goto :fail
)

echo [5/8] Adding documentation...
copy /Y "LICENSE" "%RELEASE_DIR%\LICENSE.txt" >nul
copy /Y "PRIVACY.md" "%RELEASE_DIR%\PRIVACY.md" >nul
copy /Y "PORTABLE_README.txt" "%RELEASE_DIR%\README.txt" >nul

echo [6/8] Creating Portable ZIP...
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "Compress-Archive -Path '%RELEASE_DIR%\*' -DestinationPath '%PORTABLE_ZIP%' -Force"
if errorlevel 1 goto :fail

echo [7/8] Looking for Inno Setup Compiler...
set "ISCC="
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"

if defined ISCC (
    "%ISCC%" "installer\DayTrack.iss"
    if errorlevel 1 goto :fail
) else (
    echo.
    echo [INFO] Inno Setup 6 not found. Portable ZIP is ready.
    echo Install Inno Setup and run BUILD_INSTALLER_ONLY.cmd to create Setup.exe.
    echo.
)

echo [8/8] Creating SHA-256 checksums...
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "$files=Get-ChildItem -LiteralPath '%DIST_DIR%' -File | Where-Object { $_.Extension -in '.zip','.exe' }; $lines=@(); foreach($f in $files){$h=(Get-FileHash -Algorithm SHA256 -LiteralPath $f.FullName).Hash.ToLower(); $lines += ($h + '  ' + $f.Name)}; if($lines.Count -gt 0){$lines | Set-Content -LiteralPath '%DIST_DIR%\SHA256SUMS.txt' -Encoding ASCII}"
if errorlevel 1 goto :fail

echo.
echo ==========================================================
echo                    RELEASE READY
echo ==========================================================
echo.
dir /B "%DIST_DIR%"
echo.
echo Test the v1.0.0 artifacts before uploading them publicly.
echo.
pause
exit /B 0

:fail
echo.
echo ==========================================================
echo                    RELEASE BUILD FAILED
echo ==========================================================
echo.
pause
exit /B 1
