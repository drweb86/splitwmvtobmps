@echo off

set msBuild=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set configuration=Release
set target=Rebuild

del *.zip

SET Operation=Build
"%msbuild%" "..\Bmp Imaging.sln" ^
	/Target:"%target%" ^
	/p:Configuration="%configuration%"
if %ERRORLEVEL% NEQ 0 GOTO FAILED_OPERATION;

SET Operation=Prepare Binaries
"c:\Program Files (x86)\ZipSolution-5.9\ZipSolution.Console.exe" SolutionFile=ZipSolution-bins.xml UseReleaseConfiguration
if %ERRORLEVEL% NEQ 0 GOTO FAILED_OPERATION;

SET Operation=Prepare Sources
"c:\Program Files (x86)\ZipSolution-5.9\ZipSolution.Console.exe" SolutionFile=ZipSolution-src.xml UseReleaseConfiguration
if %ERRORLEVEL% NEQ 0 GOTO FAILED_OPERATION;

GOTO END;

:FAILED_OPERATION
echo FAILED: %Operation%
pause

:END