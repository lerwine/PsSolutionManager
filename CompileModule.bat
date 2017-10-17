@ECHO OFF
SET BatchPath=%~dp0
cd "%BatchPath%"
rem powershell -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File CompileModule.ps1
powershell -STA -ExecutionPolicy Bypass -File CompileModule.ps1
pause