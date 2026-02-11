# PowerShell script to build and run Unity project
# Update the Unity executable path if needed

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
$projectPath = "C:\Users\zhaoF\OneDrive\Documents\UnityProjects\CulinaryChaos"
$buildPath = "$projectPath\Build\CulinaryChaos.exe"

# Create build directory if it doesn't exist
if (!(Test-Path "$projectPath\Build")) {
    New-Item -ItemType Directory -Path "$projectPath\Build"
}

# Run Unity build
& $unityPath -quit -batchmode -projectPath $projectPath -buildWindows64Player $buildPath

# Launch the built game
Start-Process $buildPath
