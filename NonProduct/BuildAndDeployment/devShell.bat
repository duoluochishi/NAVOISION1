@echo off

if not defined NANO_ROOT set NANO_ROOT=%cd%\..\..\

if not defined RUNTIME_MODE set RUNTIME_MODE=DEVELOPMENT

doskey devenv="%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe"
doskey build=build.bat
doskey buildall=build.bat all
doskey init=init.bat $*
doskey kill=kill.bat
doskey clean=clean.bat

cmd /k "cd %~dp0"