# Automated Unity Build using Editor Script
Write-Host "=== Automated Unity Build ===" -ForegroundColor Cyan

# Kill existing Unity processes
Write-Host "Stopping existing Unity processes..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -match "^Unity$" } | Stop-Process -Force
Start-Sleep -Seconds 2

# Start Unity with execute method to trigger build
Write-Host "Starting Unity Editor build..." -ForegroundColor Yellow
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
$projectPath = $PWD.Path
$logPath = Join-Path $projectPath "unity_editor_build.log"

$buildArgs = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "AutoBuildPlayer.BuildGame",
    "-logFile", "`"$logPath`""
)

Write-Host "Executing build..." -ForegroundColor Gray
Start-Process -FilePath $unityPath -ArgumentList $buildArgs -Wait -NoNewWindow

# Check result
Write-Host ""
Write-Host "Checking build result..." -ForegroundColor Yellow

if((Test-Path "Build\CulinaryChaos.exe") -and (Test-Path "Build\CulinaryChaos_Data")) {
    Write-Host "=== BUILD SUCCESSFUL ===" -ForegroundColor Green
    Write-Host "Launching game..." -ForegroundColor Cyan
    Start-Process -FilePath "Build\CulinaryChaos.exe"
} else {
    Write-Host "=== BUILD FAILED ===" -ForegroundColor Red
    if(Test-Path $logPath) {
        Write-Host "Last 20 lines of log:" -ForegroundColor Yellow
        Get-Content $logPath -Tail 20
    }
}
