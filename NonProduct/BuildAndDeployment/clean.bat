@echo off
echo ========= project clean tool =========
echo recursive clean  bin , obj , .vs  ...

if "%NANO_ROOT%"=="" (
    set "NANO_ROOT=%cd%"
)

REM echo %NANO_ROOT%

for /d /r %NANO_ROOT% %%d in (bin Bin obj .vs) do (
   if exist "%%d" (
       echo delete:%%d
       rmdir /s /q "%%d"
   )
)

echo clean finished
