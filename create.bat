@echo off
echo Run to script
echo Run container ...
docker run --name ms_sql_server --hostname ms_sql_server -u root -p 1400:1400 -d mcr.microsoft.com/mssql/server:2022-latest -e "ACCEPT_EULA=y" -e "SA_PASSWORD=databasePassword111Secret" -e "LD_DEBUG=1" -e "MSSQL_TCP_PORT=1400"
echo %ERRORLEVEL%
pause

if /I %ERRORLEVEL% GOTO dockerError
echo conteiner is running

echo Run script for fill the Database
cd ./DbCreationUtility
Task.Integration.Data.DbCreationUtility.exe -s "Server=172.25.208.1,1400;Database=Users;User Id=SA;Password=databasePassword111Secret;MultipleActiveResultSets=true;Encrypt=False" -p "MSSQL"
if /I %ERRORLEVEL% GOTO dockerError
echo Script 
GOTO end

:dockerError
echo Ошибка: докер не найден. Попробуйте установить докер подробнее: https://docs.docker.com/desktop/install/windows-install
GOTO end

:scriptError
echo ipconfig
echo Произошла ошибка запуска скрипта. Пожалуйста проверьте ip адрес в bat файле. Совпадает ли он с ip виртуальной машины
echo Найдите ipv4-adress для adapter ethernet vEthernet (WSL) из списка выше
echo Измените ip-адрес в строке подключения в bat файле Task.Integration.Data.DbCreationUtility.exe ... ("...Server=xxx.xxx.xxx.xxx,1400"), где x - номер ip
echo Не забудьте Изменить ip-адрес в строке подключения в проекте ("...Server=xxx.xxx.xxx.xxx,1400"), где x - номер ip
GOTO end
:end
pause