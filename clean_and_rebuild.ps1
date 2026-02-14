# Clean Unity Cache and Rebuild Script
# This fixes issues where Unity uses old cached code

Write-Host "===== CLEANING UNITY CACHE =====" -ForegroundColor Cyan

# Close Unity if running
Write-Host "Checking for running Unity processes..." -ForegroundColor Yellow
$unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcesses) {
    Write-Host "Found Unity running. Please close Unity Editor manually, then press Enter to continue..." -ForegroundColor Red
    Read-Host
}

# Delete Unity cache folders
Write-Host "`nDeleting Library folder (Unity's cache)..." -ForegroundColor Yellow
if (Test-Path "Library") {
    Remove-Item -Path "Library" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Library deleted!" -ForegroundColor Green
}

Write-Host "Deleting Temp folder..." -ForegroundColor Yellow
if (Test-Path "Temp") {
    Remove-Item -Path "Temp" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Temp deleted!" -ForegroundColor Green
}

Write-Host "Deleting old Build folder..." -ForegroundColor Yellow
if (Test-Path "Build") {
    Remove-Item -Path "Build" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Build deleted!" -ForegroundColor Green
}

Write-Host "Deleting obj folders..." -ForegroundColor Yellow
Get-ChildItem -Path "." -Directory -Filter "obj" -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n===== CACHE CLEANED! =====" -ForegroundColor Green
Write-Host "`nNow rebuilding the game with fresh cache..." -ForegroundColor Cyan

# Set paths
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
$projectPath = $PWD.Path
$buildPath = "$projectPath\Build\CulinaryChaos.exe"

# Create build directory
if (!(Test-Path "$projectPath\Build")) {
    New-Item -ItemType Directory -Path "$projectPath\Build" | Out-Null
}

Write-Host "`nBuilding game (this will take a few minutes)..." -ForegroundColor Yellow
Write-Host "Unity will reimport all assets and compile fresh code`n" -ForegroundColor Cyan

# Run Unity build
& $unityPath -quit -batchmode -projectPath $projectPath -buildWindows64Player $buildPath -logFile "$projectPath\unity_rebuild.log"

Write-Host "`n===== BUILD COMPLETE! =====" -ForegroundColor Green
Write-Host "Launching game..." -ForegroundColor Cyan

# Launch the game
if (Test-Path $buildPath) {
    Start-Process $buildPath
    Write-Host "`nGame launched! Check the new UI with all 5 tabs." -ForegroundColor Green
} else {
    Write-Host "`nBuild failed. Check unity_rebuild.log for errors." -ForegroundColor Red
}
