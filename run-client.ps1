# PowerShell script to run Unity Test Client
Write-Host "🎮 Unity Game Client Test" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion) {
        Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
        Write-Host "🔄 Restoring packages..." -ForegroundColor Yellow
        dotnet restore
        
        Write-Host "🚀 Starting client..." -ForegroundColor Yellow
        dotnet run --project UnityTestClient.csproj
    }
} catch {
    Write-Host "❌ .NET SDK not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "📥 Please install .NET SDK:" -ForegroundColor Yellow
    Write-Host "   https://dotnet.microsoft.com/download" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🎯 Alternative options:" -ForegroundColor Yellow
    Write-Host "   1. Use Visual Studio: Open UnityTestClient.csproj and press F5" -ForegroundColor White
    Write-Host "   2. Use Unity: Import UnityMatchmakingClient.cs into Unity project" -ForegroundColor White
    Write-Host "   3. Use web client: Open http://localhost:3000" -ForegroundColor White
    Write-Host ""
    
    # Try to open download page
    $choice = Read-Host "Open .NET download page? (y/n)"
    if ($choice -eq "y" -or $choice -eq "Y") {
        Start-Process "https://dotnet.microsoft.com/download"
    }
    
    Read-Host "Press Enter to exit"
}
