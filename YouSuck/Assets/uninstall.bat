@echo off

if not exist "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\YouSuck.exe" (
    echo Already uninstalled
    goto end
)

goto check_Permissions

:check_Permissions
    echo Administrative permissions required. Detecting permissions...
    
    net session >nul 2>&1
    if %errorLevel% == 0 (
        echo Success: Administrative permissions confirmed
    ) else (
        echo Failure: Current permissions inadequate. Run as Administrator
		goto end
    )
    
echo[

taskkill /IM "YouSuck.exe" /F
timeout /T 1 /nobreak >nul
del "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\YouSuck.exe"

if %errorLevel% == 0 (
    echo[
    echo Success: Uninstallation successful
) else (
    echo[
    echo Failure: Uninstallation unsuccessful
)

:end
pause