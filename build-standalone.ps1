# Script to build single-file executable for DuckDNS Updater
# Creates a standalone .exe file that can run independently

Write-Host "Building single-file executable for DuckDNS Updater..." -ForegroundColor Green

# Remove old output directory if exists
if (Test-Path "standalone-build") {
    Remove-Item "standalone-build" -Recurse -Force
    Write-Host "Removed old build directory" -ForegroundColor Yellow
}

# Build single-file executable
Write-Host "Building..." -ForegroundColor Blue
dotnet publish DuckDNSUpdater.csproj --configuration Release --runtime win-x64 --self-contained true --output "standalone-build" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none

if ($LASTEXITCODE -eq 0) {
    # Copy file to root directory
    $outputFile = "DuckDNSUpdater-Standalone.exe"
    Copy-Item "standalone-build\DuckDNSUpdater.exe" $outputFile -Force
    
    # Display file information
    $fileInfo = Get-Item $outputFile
    $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    
    Write-Host "`nSuccess!" -ForegroundColor Green
    Write-Host "File created: $outputFile" -ForegroundColor Cyan
    Write-Host "Size: $sizeMB MB" -ForegroundColor Cyan
    Write-Host "Created: $($fileInfo.LastWriteTime)" -ForegroundColor Cyan
    Write-Host "`nYou can run this file on any Windows machine without installing .NET!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
