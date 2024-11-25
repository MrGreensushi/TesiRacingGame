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



REM Avvia il gioco per il numero di giocatori specificato
for /l %%i in (1,1,%playerCount%) do (
	start RacingPrototype.exe -batchmode -nographics -client -bot -l1 -networkAddress=192.168.50.145 -logRTTOutput=C:\Users\Daniele\Desktop\Predictions\RTTs
	rem  -batchmode -nographics
	
)


rem echo Sono stati istanziati %playerCount% giocatori.
rem pause