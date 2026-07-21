@echo off
setlocal

set "ROOT=%~dp0"
set "EXE=%ROOT%publish\EditorDeClipboard.exe"

if not exist "%EXE%" (
    call "%ROOT%build.cmd"
    if errorlevel 1 exit /b %ERRORLEVEL%
)

start "" "%EXE%"
