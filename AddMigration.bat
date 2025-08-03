@echo off
set /p MigrationName="Enter migration name: "
if "%MigrationName%"=="" (
    echo Migration name cannot be empty.
    pause
    exit /b
)

:: Установить переменную окружения для скрипта
set WAREHOUSE_DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=WarehouseDB;Username=postgres;Password=postgres;Include Error Detail=true

:: Выполнить команду миграции
powershell -Command "dotnet ef migrations add %MigrationName% --project src/Warehouse.DataAccess/Warehouse.DataAccess.csproj --startup-project src/Warehouse.DataAccess/Warehouse.DataAccess.csproj --output-dir Migrations"

pause
