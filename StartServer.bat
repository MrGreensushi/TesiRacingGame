@ECHO off
setlocal enabledelayedexpansion

cd /D D:\Daniele\Github\TesiRacingGame\RacingPrototype\Builds\LatestBuild

REM Chiede la durata e la frequenza (opzionali)
set "duration="
set /p duration="Inserisci la durata (in millisecondi, lascia vuoto per 10000 ms): "
   
REM Verifica che durata e frequenza siano interi positivi solo se specificati
set "durationParam=-duration 10000"
if defined duration (
   set "var="
   for /f "delims=0123456789" %%i in ("%duration%") do set var=%%i
   if not defined var (
	echo %duration% is numeric
	set durationParam=-duration %duration%
   )
)


set "frequency="
set /p frequency="Inserisci l'intervallo di tempo tra le attivazione di SPG (in millisecondi, lascia vuoto per 13000 ms): "
   
REM Verifica che durata e frequenza siano interi positivi solo se specificati
set "frequencyParam=-frequency 13000"
if defined frequency (
   set "var="
   for /f "delims=0123456789" %%i in ("%duration%") do set var=%%i
   if not defined var (
	echo %frequency% is numeric
	set frequencyParam=-frequency %frequency%
   )
)

start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont -logOutput=C:\Users\Daniele\Desktop\Predictions -workerType=CSharpBurst %durationParam% %frequencyParam% -pythonDirectory=C:\Users\Daniele\AppData\Local\Programs\Python\Python312\python.exe  -pythonScriptPath=C:\Users\Daniele\Downloads\system_info_logger.py -percentageSPGPlayers 100 -percentageActiveSPG 50 -experimentDuration 129