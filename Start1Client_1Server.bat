@ECHO off
cd /D D:\Github\TesiRacingGame\RacingPrototype\Builds\SalvaSuFile
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 0 -y 0 -server -dont
cd..
cd Room_PredicitonOnlyWhenActivated
start RacingPrototype.exe -screen-width 960 -screen-height 540 -x 960 -y 0 -client -bot -l2