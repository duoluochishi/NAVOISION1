@echo off

call setting.bat

for /f "delims=" %%a in ('dir *.sql /b /o:-d "%db_path%"') do (
	set latest_file=%%a
	goto :found
)
goto :end

:found
set MYSQL_EXE=%MYSQL_BIN%\mysql.exe

"%MYSQL_EXE%" -h%db_host% -u%db_user% %db_name% < %cd%\%latest_file%

@echo done!

:end
exit
