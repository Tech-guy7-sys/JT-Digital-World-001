@echo off
setlocal
cd /d "%~dp0"
color 0B
cls
echo ============================================================
echo                 J TECH DIGITAL WORLD v60.2
echo                 WINDOWS SETUP CENTER
echo ============================================================
echo.
echo [1] Run the graphical Setup Wizard
echo [2] Build all 9 Windows installers
echo [3] Start the public website locally
echo [4] Run verification checks
echo [5] Exit
echo.
choice /c 12345 /n /m "Select an option: "
if errorlevel 5 exit /b 0
if errorlevel 4 goto VERIFY
if errorlevel 3 goto WEBSITE
if errorlevel 2 goto BUILD
if errorlevel 1 goto WIZARD
:WIZARD
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup-Wizard\JT-Setup-Wizard.ps1"
goto END
:BUILD
call "%~dp0Build\BUILD-ALL-JT-APPS.bat"
goto END
:WEBSITE
call "%~dp0Website\RUN-WEBSITE-IN-EDGE.bat"
goto END
:VERIFY
cd /d "%~dp0Build"
call npm run check:syntax
call npm run verify:all
pause
goto END
:END
endlocal
