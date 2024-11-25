@echo off
:: Nome del processo da terminare
set "processName=RacingPrototype.exe"

echo Terminazione di tutti i processi con nome %processName%...

:: Utilizza il comando taskkill per terminare i processi
taskkill /f /im "%processName%" >nul 2>&1

if %errorlevel% equ 0 (
    echo Tutti i processi "%processName%" sono stati terminati con successo.
) else (
    echo Nessun processo con nome "%processName%" trovato o permessi insufficienti.
)

pause