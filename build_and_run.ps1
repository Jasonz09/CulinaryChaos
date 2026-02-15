# PowerShell script to build and run Unity project
# Update the Unity executable path if needed

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = "D:\UnityProjects\CulinaryChaos"
$buildPath = "$projectPath\Build\CulinaryChaos.exe"
$logFile   = "$projectPath\unity_build.log"

# Create build directory if it doesn't exist
if (!(Test-Path "$projectPath\Build")) {
    New-Item -ItemType Directory -Path "$projectPath\Build"
}

Write-Host "=== Unity Build Starting ==="
Write-Host "Unity:   $unityPath"
Write-Host "Project: $projectPath"
Write-Host "Output:  $buildPath"
Write-Host "Log:     $logFile"
Write-Host ""

# Run Unity build with explicit log file, wait for exit
$proc = Start-Process -FilePath $unityPath `
    -ArgumentList "-quit","-batchmode","-projectPath",$projectPath,"-buildWindows64Player",$buildPath,"-logFile",$logFile `
    -Wait -PassThru -NoNewWindow

Write-Host ""
Write-Host "=== Unity exited with code $($proc.ExitCode) ==="

if ($proc.ExitCode -ne 0) {
    Write-Host "BUILD FAILED — check $logFile for details"
    Write-Host "--- Last 30 lines of log ---"
    Get-Content $logFile -Tail 30
    exit 1
}

if (!(Test-Path $buildPath)) {
    Write-Host "BUILD FAILED — $buildPath not found after build"
    exit 1
}

Write-Host "Build succeeded. Launching game..."
Start-Process $buildPath
