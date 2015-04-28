@echo off
setlocal
set ASSEMBLY_NAME=CSharpFormat

if exist %SYSTEMROOT%\Microsoft.NET\Framework\v2.0.50727 goto net20
if exist %SYSTEMROOT%\Microsoft.NET\Framework\v1.1.4322 goto net11
if exist %SYSTEMROOT%\Microsoft.NET\Framework\v1.0.3705 goto net10

echo Error: .NET Framework 2.0, 1.1 or 1.0 required.
echo.
goto end 

:net10
set CSC_PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v1.0.3705
goto options

:net11
set CSC_PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v1.1.4322
goto options

:net20
set CSC_PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v2.0.50727
goto options

:options
if %1x == -debugx goto debug
goto release

:release
set OUTPATH=Release
goto compile

:debug
set OUTPATH=Debug
set DEBUG="/debug"
goto compile

:compile
@echo Compiling %ASSEMBLY_NAME%.dll...
@echo.

if not exist bin md bin
if not exist bin\%OUTPATH% md bin\%OUTPATH%
if exist bin\%OUTPATH%\%ASSEMBLY_NAME%.* del bin\%OUTPATH%\%ASSEMBLY_NAME%.* /Q

%CSC_PATH%\csc %DEBUG% /target:library /doc:bin\%OUTPATH%\%ASSEMBLY_NAME%.xml /out:bin\%OUTPATH%\%ASSEMBLY_NAME%.dll *.cs /resource:csharp.css

echo.
echo Done.
:end
pause
