@ECHO off
setlocal enabledelayedexpansion

cd /D D:\Github\TesiRacingGame\RacingPrototype\Builds\LatestBuild

REM Chiede il numero di giocatori
set /p playerCount="Quanti giocatori istanziare? (Inserisci un numero intero): "

REM Controlla se il numero inserito è valido
set /a "test=%playerCount%" >nul 2>&1
if %errorlevel% neq 0 (
    echo Errore: inserisci un numero intero valido.
    pause
    exit /b
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
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont

REM Avvia il gioco per il numero di giocatori specificato
for /l %%i in (1,1,%playerCount%) do (
	start RacingPrototype.exe -screen-width %screenWidth% -screen-height %screenHeight% -x !xPos! -y !yPos! -client -bot -l2
	
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


echo Sono stati istanziati %playerCount% giocatori.
pause