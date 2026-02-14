# Build Unity project with OneDrive completely disabled
Write-Host "=== Building Unity Project (OneDrive Disabled) ===" -ForegroundColor Cyan

# 1. Kill any running Unity processes
Write-Host ""
Write-Host "[1/6] Stopping Unity processes..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -match "^Unity$" } | Stop-Process -Force
Start-Sleep -Seconds 2

# 2. Stop OneDrive
Write-Host "[2/6] Stopping OneDrive..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -eq "OneDrive" } | Stop-Process -Force
Start-Sleep -Seconds 3

# 3. Delete Library and Temp to force clean rebuild
Write-Host "[3/6] Deleting cache folders..." -ForegroundColor Yellow
if(Test-Path "Library") { Remove-Item "Library" -Recurse -Force }
if(Test-Path "Temp") { Remove-Item "Temp" -Recurse -Force }
if(Test-Path "Build") { Remove-Item "Build" -Recurse -Force }
Start-Sleep -Seconds 2

# 4. Start Unity build (batchmode)
Write-Host "[4/6] Starting Unity build (this will take several minutes)..." -ForegroundColor Yellow
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.27f1\Editor\Unity.exe"
$projectPath = $PWD.Path
$logPath = Join-Path $projectPath "unity_clean_build.log"

$buildArgs = @(
    "-quit",
    "-batchmode",
    "-nographics",
    "-projectPath", "`"$projectPath`"",
    "-buildWindows64Player", "`"$projectPath\Build\CulinaryChaos.exe`"",
    "-logFile", "`"$logPath`""
)

Write-Host "Unity command: $unityPath $buildArgs" -ForegroundColor Gray
Start-Process -FilePath $unityPath -ArgumentList $buildArgs -Wait -NoNewWindow

# 5. Check build result
Write-Host ""
Write-Host "[5/6] Checking build result..." -ForegroundColor Yellow
if(Test-Path "Build\CulinaryChaos.exe") {
    Write-Host "Executable found" -ForegroundColor Green
} else {
    Write-Host "Executable NOT found" -ForegroundColor Red
}

if(Test-Path "Build\CulinaryChaos_Data") {
    Write-Host "Data folder found" -ForegroundColor Green
    Write-Host ""
    Write-Host "=== BUILD SUCCESSFUL ===" -ForegroundColor Green
} else {
    Write-Host "Data folder NOT found" -ForegroundColor Red
    Write-Host ""
    Write-Host "=== BUILD FAILED - Check unity_clean_build.log ===" -ForegroundColor Red
}

# 6. Restart OneDrive
Write-Host ""
Write-Host "[6/6] Restarting OneDrive..." -ForegroundColor Yellow
Start-Process -FilePath "$env:LOCALAPPDATA\Microsoft\OneDrive\OneDrive.exe" -WindowStyle Hidden
Write-Host "OneDrive restarted" -ForegroundColor Green

Write-Host ""
Write-Host "=== SCRIPT COMPLETE ===" -ForegroundColor Cyan
