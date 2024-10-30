@ECHO off

rem cd D:\Github\TesiRacingGame\RacingPrototype\Builds\Room_server

rem timeout /t 1 

cd /D D:\Github\TesiRacingGame\RacingPrototype\Builds\Room_PredicitonOnlyWhenActivated
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont

rem start RacingPrototype.exe -screen-width 1920 -screen-height 1080 -x 0 -y 0 -client -dont -path=C:\Users\dansp\OneDrive\Desktop\test.txt -name Player


start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -dont -name Bot0
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -dont -name Bot1
rem start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -dont -name Bot2



cd D:\Github\TesiRacingGame\RacingPrototype\Builds\Room_server

