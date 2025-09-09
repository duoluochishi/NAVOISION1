@echo off

if not defined NANO_ROOT (
	set NANO_ROOT=%cd%\..\..\
)

@echo RUNTIME_MODE:%RUNTIME_MODE%

if not exist %NANO_ROOT%bin mkdir %NANO_ROOT%bin
xcopy %NANO_ROOT%References %NANO_ROOT%bin /Y/E /r/f 

xcopy %NANO_ROOT%References\ImageDll %NANO_ROOT%bin /Y /E /r/f               
rd /s/q %NANO_ROOT%bin\ImageDll

timeout /t 2 /nobreak > NUL

set NOWARNING=MSB3202,MSB3270,NU1803,NU1503,CA1416,CA2200,CS0067,CS8600,CS8601,CS8602,CS8603,CS8604,CS8618,CS8620,CS8622,CS8625,CS8629,CS8631,CS8633,CS8766
set NOWARNING1=MSB3202,MSB3270,NU1803,NU1503,CA1416,CA2200,CS0067,CS1587,CS8600,CS8601,CS8602,CS8603,CS8604,CS8618,CS8620,CS8622,CS8625,CS8629,CS8631,CS8632,CS8633,CS8766
::CS8612,CS8615
::CS8765,CS8767,CS8769

dotnet clean %NANO_ROOT%CompilingManager\CompilingManager.sln
dotnet build -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompilingManager\CompilingManager.sln

%NANO_ROOT%bin\CompleteSolution.exe

timeout /t 3 /nobreak > NUL

dotnet clean %NANO_ROOT%CompleteSkin.sln
dotnet clean %NANO_ROOT%CompleteMCS.sln
dotnet clean %NANO_ROOT%CompleteService.sln

if not defined RUNTIME_MODE (
	@echo dotnet publish skin
	dotnet build -c Release -nowarn:%NOWARNING1% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteSkin.sln
)
if defined RUNTIME_MODE (
	@echo dotnet build skin
	dotnet build -nowarn:%NOWARNING1% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteSkin.sln
)

if "%1"=="" goto buildmcs
if %1==all goto buildcompletion

:buildmcs
if not defined RUNTIME_MODE (
	@echo dotnet publish mcs
	dotnet build -c Release -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteMCS.sln
)
if defined RUNTIME_MODE (
	@echo dotnet build mcs
	dotnet build -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteMCS.sln
)
goto end

:buildcompletion
if not defined RUNTIME_MODE (
	@echo dotnet publish all
	dotnet build -c Release -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteMCS.sln
	dotnet build -c Release -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteService.sln
)
if defined RUNTIME_MODE (
	@echo dotnet build all
	dotnet build -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteMCS.sln
	dotnet build -nowarn:%NOWARNING% --property:OutputPath=%NANO_ROOT%bin %NANO_ROOT%CompleteService.sln
)

:end
::if defined RUNTIME_MODE (if not exist %NANO_ROOT%Logs mkdir %NANO_ROOT%Logs)
::if defined RUNTIME_MODE (if not exist %NANO_ROOT%AppData mkdir %NANO_ROOT%AppData)

del /f /s /q %NANO_ROOT%CompleteSkin.sln
del /f /s /q %NANO_ROOT%CompleteMCS.sln
del /f /s /q %NANO_ROOT%CompleteService.sln
if exist %NANO_ROOT%bin\CompleteSolution.exe del /f /s /q %NANO_ROOT%bin\CompleteSolution.*