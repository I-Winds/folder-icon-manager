@echo off
REM Folder Icon Manager — 构建脚本
REM 设置 MinGW 路径并执行 Tauri 构建

set "PATH=C:\msys64\mingw64\bin;C:\Users\IWindL\.cargo\bin;%PATH%"
set "CARGO_TERM_COLOR=always"

cd /d "%~dp0..\src-tauri"

echo ========================================
echo Folder Icon Manager — Cargo Build
echo ========================================
cargo build %*
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
