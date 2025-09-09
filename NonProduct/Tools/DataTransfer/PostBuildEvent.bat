@echo off
@REM Rename the exe name to avoid "kill" command

if exist "%~1DataTransferTool.exe" (
    del "%~1DataTransferTool.exe"
)

if exist "%~1NV.CT.NP.Tools.DataTransfer.exe" (
    ren "%~1NV.CT.NP.Tools.DataTransfer.exe" "DataTransferTool.exe"
)
