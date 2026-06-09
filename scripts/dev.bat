@echo off
REM Folder Icon Manager — 开发模式
REM 前端 dev server 已由 tauri.conf.json 自动管理 (beforeDevCommand)

set "PATH=C:\msys64\mingw64\bin;C:\Users\IWindL\.cargo\bin;%PATH%"

cd /d "%~dp0.."
echo ========================================
echo Folder Icon Manager — Dev Mode
echo ========================================
pnpm tauri dev
