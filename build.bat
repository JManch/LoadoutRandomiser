@echo off
SETLOCAL

CALL :PlatformBuild win-x64
CALL :PlatformBuild win-x86
CALL :PlatformBuild linux-x64

EXIT /B 0

:PlatformBuild
dotnet publish --runtime %~1 --self-contained
IF EXIST "%cd%\builds\LoadoutRandomiser.%~1.zip\" DEL /Q "%cd%\builds\LoadoutRandomiser.%~1.zip\"
set executable=LoadoutRandomiser.exe
IF %~1==linux-x64 set executable=LoadoutRandomiser
"C:\Program Files\7-Zip\7z.exe" a "%cd%\builds\LoadoutRandomiser.%~1.zip" "%cd%\LoadoutData\" "%cd%\bin\Debug\net6.0\%~1\publish\%executable%"