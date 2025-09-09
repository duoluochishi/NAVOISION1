@echo off

if not defined NANO_ROOT set NANO_ROOT=%cd%\..\..\

start %NANO_ROOT%bin\NV.CT.LoggingServer.exe

timeout /t 1 /nobreak > NUL

start %NANO_ROOT%bin\NV.CT.MCS.Services.exe

timeout /t 1 /nobreak > NUL

start %NANO_ROOT%bin\NV.CT.JobService.exe

start %NANO_ROOT%bin\NV.CT.SyncService.exe

timeout /t 1 /nobreak > NUL

if "%1"=="" start %NANO_ROOT%bin\NV.CT.NanoConsole.exe

timeout /t 1 /nobreak > NUL

if "%1"=="" start %NANO_ROOT%bin\NV.CT.AuxConsole.exe

timeout /t 1 /nobreak > NUL

if %1==PatientBrowser start %NANO_ROOT%bin\NV.CT.PatientBrowser.exe

if %1==PatientManagement start %NANO_ROOT%bin\NV.CT.PatientManagement.exe

if %1==ProtocolManagement start %NANO_ROOT%bin\NV.CT.ProtocolManagement.exe

if %1==Examination start %NANO_ROOT%bin\NV.CT.Examination.exe

if %1==ServiceFrame start %NANO_ROOT%bin\NV.CT.ServiceFrame.exe

if %1==JobViewer start %NANO_ROOT%bin\NV.CT.JobViewer.exe

timeout /t 1 /nobreak > NUL