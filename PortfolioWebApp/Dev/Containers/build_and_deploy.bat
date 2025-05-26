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
    call :deploy_db || exit /b 1
) else if "%target%"=="webapp" (
    call :deploy_webapp || exit /b 1
) else if "%target%"=="all" (
    call :deploy_db || exit /b 1
    call :deploy_webapp || exit /b 1
    docker ps --filter "name=blazor-webapp|database-webapp" --format "table {{.Names}}    {{.Status}}    {{.Ports}}"
) else (
    echo Invalid argument: %target%
    call :show_help
)
exit /b 0

:show_help
echo Verwendung: %~n0 [db^|webapp^|all]
echo   db      - Builds and deploys the database
echo   webapp  - Builds and deploys the webapp
echo   all     - Builds and deploys both (default)
exit /b 1

:deploy_db
echo Starting database deployment...
echo Removing old containers...
docker-compose down -v --remove-orphans >nul 2>&1
docker volume rm portfoliowebapp_postgres_data >nul 2>&1

echo Building database...
docker-compose build --no-cache db
if %errorlevel% neq 0 (
    echo Database build failed
    exit /b 1
)

echo Starting database container...
docker-compose up -d db
if %errorlevel% neq 0 (
    echo Database startup failed
    docker-compose logs db
    exit /b 1
)

echo Waiting for database (max 30s)...
set timeout=30
set counter=0

:wait_loop
docker-compose exec db pg_isready -U webapp -d webappdb >nul 2>&1
if %errorlevel% equ 0 goto db_ready

timeout /t 1 >nul
set /a counter+=1
if !counter! geq %timeout% (
    echo Database not ready after %timeout% seconds
    docker-compose logs db
    exit /b 1
)
goto wait_loop

:db_ready
echo Database ready after !counter! seconds
echo Port: 6543 -> 5432
echo User: webapp/webapp123
echo DB: webappdb
exit /b 0

:deploy_webapp
echo Starting webapp deployment...
docker-compose up -d --build web
if %errorlevel% neq 0 (
    echo Webapp deployment failed
    exit /b 1
)

echo Webapp deployed
echo URL: http://localhost:5120
exit /b 0