@echo off

set MYSQL_PWD=NV123456
set dbhost=127.0.0.1
set dbuser=root
set dbname=db_mcs
set dbsource=%cd%\source_database.sql
set dbtables=%cd%\source_tables.sql

set MYSQL_BIN=%ProgramFiles%\MySQL\MySQL Server 8.0\bin

if not exist "%MYSQL_BIN%" (
    echo %MYSQL_BIN% does not exist!
)
