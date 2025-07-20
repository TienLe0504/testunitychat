@echo off
echo 🎮 Simple Game Client Test
echo ========================

REM Try different ways to run dotnet
dotnet --version >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ .NET SDK found in PATH
    dotnet run --project SimpleClient.csproj
) else (
    echo ❌ .NET SDK not in PATH, trying direct path...
    
    REM Try Program Files path
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        echo ✅ Found .NET at C:\Program Files\dotnet\
        "C:\Program Files\dotnet\dotnet.exe" run --project SimpleClient.csproj
    ) else (
        echo ❌ .NET SDK not found!
        echo Please install .NET SDK from: https://dotnet.microsoft.com/download
        pause
    )
)
