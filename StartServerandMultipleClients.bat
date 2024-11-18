@ECHO off
setlocal enabledelayedexpansion

cd /D D:\Daniele\Github\TesiRacingGame\RacingPrototype\Builds\LatestBuild

REM Chiede il numero di giocatori
set /p playerCount="Quanti giocatori istanziare? (Inserisci un numero intero): "

REM Controlla se il numero inserito è valido
set /a "test=%playerCount%" >nul 2>&1
if %errorlevel% neq 0 (
    echo Errore: inserisci un numero intero valido.
    pause
    exit /b
)

REM Chiede la durata e la frequenza (opzionali)
set "duration="
set /p duration="Inserisci la durata (in millisecondi, lascia vuoto per 300 ms): "
   
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

REM Imposta le dimensioni della finestra e gli incrementi di posizione
set screenWidth=960
set screenHeight=540
set xIncrement=100
set yIncrement=100

REM Avvia il gioco per il numero di giocatori specificato, con posizione diversa per ogni finestra
set /a xPos=100
set /a yPos=100

set /a maxX=1920 - (%screenWidth%/2)
set /a maxY=1080 - (%screenHeight%/2)
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont -logOutput=C:\Users\Daniele\Desktop\Predictions -workerType=ComputePrecompiled %durationParam% -pythonDirectory=C:\Users\Daniele\AppData\Local\Programs\Python\Python312\python.exe  -pythonScriptPath=C:\Users\Daniele\Downloads\system_info_logger.py -processName= 
 

rem timeout /t 5 /nobreak

REM Avvia il gioco per il numero di giocatori specificato
for /l %%i in (1,1,%playerCount%) do (
	start RacingPrototype.exe -screen-width %screenWidth% -screen-height %screenHeight% -x !xPos! -y !yPos! -client -bot -l1 
	rem -networkAddress=192.168.50.3
	rem  -batchmode -nographics
	
	REM Aggiorna la posizione per la prossima finestra
	set /a xPos=xPos+%xIncrement%
    set /a yPos=yPos+%yIncrement%
	REM Controlla se la posizione supera i limiti e, se sì, la reimposta
	if !xPos! gtr %maxX% (
		set /a xPos=0
	) 
	if !yPos! gtr %maxY% (
		set /a yPos=0
	)

	echo %%i, !xPos! !yPos!
)


rem echo Sono stati istanziati %playerCount% giocatori.
rem pause