@echo off
echo -----------------------------------------------
echo Run to script
echo Run container ...
docker run --name ms_sql_server --hostname ms_sql_server -u root -p 1400:1400 -e "ACCEPT_EULA=y" -e "SA_PASSWORD=databasePassword111Secret" -e "LD_DEBUG=1" -e "MSSQL_TCP_PORT=1400" -d mcr.microsoft.com/mssql/server:2022-latest

set /A TRUE=1
IF %ERRORLEVEL% EQU 0 set /A TRUE=0
IF %ERRORLEVEL% EQU 125 set /A TRUE=0
IF %TRUE% NEQ 0 GOTO dockerError
echo conteiner is running
echo -----------------------------------------------

echo waiting run database 5 secund
timeout 5

echo -----------------------------------------------
echo Run script for fill the database
cd ./DbCreationUtility
Task.Integration.Data.DbCreationUtility.exe -s "Server=172.25.208.1,1400;Database=Users;User Id=SA;Password=databasePassword111Secret;MultipleActiveResultSets=true;Encrypt=False" -p "MSSQL"
if /I %ERRORLEVEL% NEQ 0 GOTO scriptError
echo end script for fill the database
echo -----------------------------------------------
GOTO end

:dockerError
echo Error: docker not found. Try installing docker for more details: https://docs.docker.com/desktop/install/windows-install
GOTO end

:scriptError
echo -----------------------------------------------
ipconfig
echo -----------------------------------------------
echo An error occurred while running the script. Please check the IP address in the bat file. Does it match the virtual machine's IP?
echo Find the ipv4-adress for adapter ethernet vEthernet (WSL) from the list above
echo Change the ip address in the connection line in the bat file Task.Integration.Data.DbCreationUtility.exe ... ("...Server=xxx.xxx.xxx.xxx,1400"), where x is the ip number
echo Don't forget to Change the IP address in the connection line in the project ("...Server=xxx.xxx.xxx.xxx,1400"), where x is the ip number
GOTO end
:end
pause