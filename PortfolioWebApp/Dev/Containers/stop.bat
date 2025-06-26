@echo off
setlocal

set PROJECT_ROOT=%~dp0..

:main
cd /d "%PROJECT_ROOT%" || (
    echo Error: Project root not found
    exit /b 1
)

set target=%1
if "%target%"=="" set target=all

if "%target%"=="db" (
    call :stop_db || exit /b 1
) else if "%target%"=="webapp" (
    call :stop_webapp || exit /b 1
) else if "%target%"=="all" (
    call :stop_db || exit /b 1
    call :stop_webapp || exit /b 1
    echo All services stopped successfully
) else if "%target%"=="-h" (
    call :show_help
) else if "%target%"=="--help" (
    call :show_help
) else (
    echo Invalid argument: %target%
    call :show_help
)
exit /b 0

:show_help
echo Usage: %~n0 [db^|webapp^|all]
echo   db      - Stop only the database
echo   webapp  - Stop only the webapp
echo   all     - Stop everything (default)
exit /b 1

:stop_db
echo Stopping database...
docker-compose stop db
if %errorlevel% neq 0 (
    echo Failed to stop database
    exit /b 1
)
echo Database stopped
exit /b 0

:stop_webapp
echo Stopping webapp...
docker-compose stop web
if %errorlevel% neq 0 (
    echo Failed to stop webapp
    exit /b 1
)
echo Webapp stopped
exit /b 0