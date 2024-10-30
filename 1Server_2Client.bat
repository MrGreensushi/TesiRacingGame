@ECHO off

cd D:\Github\TesiRacingGame\RacingPrototype\Builds\Room_server

timeout /t 1 


start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont

start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -l2 -name Bot0
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -l2 -name Bot1
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -l2 -name Bot2

cd /D D:\Github\TesiRacingGame\RacingPrototype\Builds\Room_PredicitonOnlyWhenActivated
start RacingPrototype.exe -screen-width 1920 -screen-height 1080 -x 0 -y 0 -client -dont -path=C:\Users\dansp\OneDrive\Desktop\Enrico\l2_1.txt -name Player

