@echo off
setlocal enableextensions enabledelayedexpansion
set SCRIPT_DIR=%~dp0
pushd "%SCRIPT_DIR%"

for %%D in (SnbtCmd Nbt fNbt Utility) do (
  if exist "%%D\bin" rmdir /s /q "%%D\bin"
  if exist "%%D\obj" rmdir /s /q "%%D\obj"
)
if exist "publish" rmdir /s /q "publish"

echo Clean complete.
popd
endlocal
exit /b 0
