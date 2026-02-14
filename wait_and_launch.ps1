# Wait for Unity build to complete and auto-launch the game

Write-Host "===== WAITING FOR BUILD TO COMPLETE =====" -ForegroundColor Cyan
Write-Host "Unity is rebuilding the game with fresh cache..." -ForegroundColor Yellow
Write-Host "This typically takes 3-5 minutes`n" -ForegroundColor Gray

$buildPath = "$PWD\Build\CulinaryChaos.exe"
$dataPath = "$PWD\Build\CulinaryChaos_Data"

$dots = 0
while ($true) {
    # Check if Unity process is still running
    $unityRunning = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    
    # Check if build is complete
    $buildComplete = (Test-Path $buildPath) -and (Test-Path $dataPath)
    
    if ($buildComplete -and -not $unityRunning) {
        Write-Host "`n`n===== BUILD COMPLETE! =====" -ForegroundColor Green
        Write-Host "Launching game in 2 seconds..." -ForegroundColor Cyan
        Start-Sleep -Seconds 2
        Start-Process $buildPath
        Write-Host "`nGame launched! Enjoy your updated UI!" -ForegroundColor Green
        break
    }
    
    # Show progress indicator
    $dots = ($dots + 1) % 4
    $progress = "." * $dots + " " * (3 - $dots)
    Write-Host "`rBuilding$progress" -NoNewline -ForegroundColor Yellow
    
    Start-Sleep -Seconds 2
    
    # Show elapsed time every 10 seconds
    if ((Get-Date).Second % 10 -eq 0) {
        if ($unityRunning) {
            $runtime = (Get-Date) - $unityRunning.StartTime
            Write-Host "`rElapsed: $($runtime.Minutes)m $($runtime.Seconds)s  " -NoNewline -ForegroundColor Gray
        }
    }
}
