@ECHO off
cd /D D:\Github\TesiRacingGame\RacingPrototype\Builds\NoLatency
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -bot 
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client 
cd ..
cd Build
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 540 -client