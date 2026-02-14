# Quick Manual Build - Open Unity Editor, let it fix packages, build, and exit
Write-Host "Opening Unity Editor to resolve packages..." -ForegroundColor Cyan

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.27f1\Editor\Unity.exe"
$projectPath = $PWD.Path

# Open Unity Editor (non-batchmode) to let it auto-fix package issues
Write-Host "Starting Unity Editor - please wait for it to load completely..."
$editor = Start-Process -FilePath $unityPath -ArgumentList "-projectPath `"$projectPath`"" -PassThru

# Wait for Unity to fully initialize (check log file for completion signs)
Write-Host "Waiting for Unity to initialize packages..."
$maxWait = 180 # 3 minutes
$elapsed = 0
while($elapsed -lt $maxWait) {
    Start-Sleep -Seconds 5
    $elapsed += 5
    
    if(Test-Path "Library\PackageCache\com.unity.collections@*") {
        Write-Host "Packages seem to be resolving..."
    }
    
    # Check if editor is responsive
    if($editor.HasExited) {
        Write-Host "Unity Editor exited unexpectedly" -ForegroundColor Red
        break
    }
    
    Write-Host "." -NoNewline
}

Write-Host ""
Write-Host "Unity should now be open. Please:"
Write-Host "1. Wait for all scripts to compile (check bottom-right of Unity window)"
Write-Host "2. Go to File > Build Settings"
Write-Host "3. Click 'Build' and select the 'Build' folder"
Write-Host "4. Wait for build to complete"
Write-Host "5. Close Unity when done"
