@echo off

docker compose -f docker-compose.postgres.yml up -d
if %ERRORLEVEL% GEQ 1 EXIT /B 1

timeout /t 3

DbCreationUtility\Task.Integration.Data.DbCreationUtility -s "Server=localhost;Port=8080;Database=testDb;Username=testUser;Password=123;" -p "POSTGRE"