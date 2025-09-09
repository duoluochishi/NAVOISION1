@echo off

:: MySQL 登录信息
set dbhost=127.0.0.1
set dbuser=root
set MYSQL_PWD=NV123456

:: 数据库信息
set dbname=db_mcs
set dbsource=%cd%\source_database.sql
set dbtables=%cd%\source_tables.sql
set dbdata=%cd%\source_init_data.sql
set check_table=t_study
set characterset=utf8mb4

:: MySQL运行
set MYSQL_BIN=%ProgramFiles%\MySQL\MySQL Server 8.0\bin
set MYSQL_EXE=%MYSQL_BIN%\mysql.exe

if not exist "%MYSQL_BIN%" (
	echo MySQL does not exist.
	goto :end
)

:: 检查数据库是否存在
"%MYSQL_EXE%" -h %dbhost% -u %dbuser% -e "USE %dbname%;" > nul 2>&1

if %ERRORLEVEL% == 0 (
	echo Database %dbname% exists
	goto :findTables
) else (
	echo Database %dbname% does not exist.
	goto :createDatabase
)

:: 创建数据库
:createDatabase
	"%MYSQL_EXE%" -h %dbhost% -u %dbuser% --default-character-set=%characterset% <%dbsource%
	goto :createTables

:: 检查数据表是否存在
:findTables
	"%MYSQL_EXE%" -h %dbhost% -u %dbuser% -e "USE %dbname%; SHOW TABLES LIKE '%check_table%';" | findstr /i %check_table% > nul 2>&1
	if %ERRORLEVEL% == 0 (
		echo Table %check_table% exist in database %dbname%.
		goto :end
	) else (
		echo Table %check_table% does not exist in database %dbname%.
		goto :createTables
	)
goto :end

:: 创建数据表并初始化数据
:createTables
	echo Create Tables
	"%MYSQL_EXE%" -h %dbhost% -u %dbuser% --default-character-set=%characterset% <%dbtables%
	echo Initialize Data
	"%MYSQL_EXE%" -h %dbhost% -u %dbuser% --default-character-set=%characterset% <%dbdata%
	echo Done!

:end
exit