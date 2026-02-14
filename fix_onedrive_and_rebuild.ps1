# Fix OneDrive File Lock Issue and Rebuild

Write-Host "===== FIXING ONEDRIVE LOCK ISSUE =====" -ForegroundColor Cyan

# Step 1: Close Unity
Write-Host "`nStep 1: Closing Unity..." -ForegroundColor Yellow
$unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcesses) {
    $unityProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "Unity closed!" -ForegroundColor Green
}

# Step 2: Stop OneDrive temporarily
Write-Host "`nStep 2: Stopping OneDrive to release file locks..." -ForegroundColor Yellow
$oneDriveProcesses = Get-Process -Name "OneDrive" -ErrorAction SilentlyContinue
if ($oneDriveProcesses) {
    $oneDriveProcesses | Stop-Process -Force
    Start-Sleep -Seconds 3
    Write-Host "OneDrive stopped!" -ForegroundColor Green
}

# Step 3: Delete Library folder (now that files are unlocked)
Write-Host "`nStep 3: Deleting Library folder..." -ForegroundColor Yellow
if (Test-Path "Library") {
    Remove-Item -Path "Library" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Library deleted!" -ForegroundColor Green
}

if (Test-Path "Temp") {
    Remove-Item -Path "Temp" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Temp deleted!" -ForegroundColor Green
}

# Step 4: Restart OneDrive
Write-Host "`nStep 4: Restarting OneDrive..." -ForegroundColor Yellow
Start-Process "$env:LOCALAPPDATA\Microsoft\OneDrive\OneDrive.exe"
Start-Sleep -Seconds 2
Write-Host "OneDrive restarted!" -ForegroundColor Green

# Step 5: Rebuild the game
Write-Host "`n===== BUILDING GAME =====" -ForegroundColor Cyan

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
$projectPath = $PWD.Path
$buildPath = "$projectPath\Build\CulinaryChaos.exe"

if (!(Test-Path "$projectPath\Build")) {
    New-Item -ItemType Directory -Path "$projectPath\Build" | Out-Null
}

Write-Host "Starting Unity build (this will take 3-5 minutes)..." -ForegroundColor Yellow
& $unityPath -quit -batchmode -projectPath $projectPath -buildWindows64Player $buildPath -logFile "$projectPath\unity_final_build.log"

Write-Host "`n===== BUILD COMPLETE! =====" -ForegroundColor Green

if (Test-Path $buildPath) {
    Write-Host "Launching game..." -ForegroundColor Cyan
    Start-Process $buildPath
    Write-Host "`nGame launched successfully!" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed. Check unity_final_build.log for details." -ForegroundColor Red
}
