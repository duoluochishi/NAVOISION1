@echo off

call setting.bat

for /f "tokens=2 delims==" %%I in ('"wmic os get localdatetime /value"') do set datetime=%%I
set timestamp=%datetime:~0,4%%datetime:~4,2%%datetime:~6,2%%datetime:~8,2%%datetime:~10,2%%datetime:~12,2%
set sqlfile=%cd%\db_mcs.%timestamp%.sql

cd /d %sql_bin%

mysqldump -h%dbhost% -u%dbuser% -p%MYSQL_PWD% %dbname% >%sqlfile%

@echo done!

exit