@echo off
setlocal enabledelayedexpansion

set PROJECT_ROOT=%~dp0..

:main
cd /d "%PROJECT_ROOT%" || (
    echo Error: Project root not found
    exit /b 1
)

set target=%1
if "%target%"=="" set target=all

if "%target%"=="db" (
    call :restart_db || exit /b 1
) else if "%target%"=="webapp" (
    call :restart_webapp || exit /b 1
) else if "%target%"=="all" (
    call :restart_db || exit /b 1
    call :restart_webapp || exit /b 1
    echo All services restarted successfully
    docker ps --filter "name=blazor-webapp|database-webapp" --format "table {{.Names}}    {{.Status}}    {{.Ports}}"
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
echo   db      - Restart only the database
echo   webapp  - Restart only the webapp
echo   all     - Restart everything (default)
exit /b 1

:restart_db
echo Restarting database...
docker-compose restart db
if %errorlevel% neq 0 (
    echo Failed to restart database
    exit /b 1
)

echo Waiting for database (max 10s)...
set timeout=10
set counter=0

:wait_loop
docker exec database-webapp pg_isready -U webapp -d webappdb >nul 2>&1
if %errorlevel% equ 0 goto db_ready

timeout /t 1 >nul
set /a counter+=1
if !counter! geq %timeout% (
    echo Database not ready after %timeout% seconds
    exit /b 1
)
goto wait_loop

:db_ready
echo Database ready after !counter! seconds
exit /b 0

:restart_webapp
echo Restarting webapp...
docker-compose restart web
if %errorlevel% neq 0 (
    echo Failed to restart webapp
    exit /b 1
)

echo Webapp restarted
exit /b 0