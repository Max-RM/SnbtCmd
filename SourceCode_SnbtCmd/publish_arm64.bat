@echo off
setlocal enableextensions enabledelayedexpansion
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"
set CONFIG=Release
set RID=win-arm64
set OUTDIR=%SCRIPT_DIR%publish\%RID%\
call dotnet restore "SnbtCmd.sln" || goto :error
call dotnet publish "SnbtCmd\SnbtCmd.csproj" -c %CONFIG% -r %RID% -o "%OUTDIR%" --no-restore || goto :error
echo.
echo Published to "%OUTDIR%"
popd
endlocal
exit /b 0
:error
echo Build failed with error %errorlevel%.
popd
endlocal
exit /b %errorlevel%
