# Exclude Unity Cache Folders from OneDrive
# This prevents the "delete 1000 items" popup

Write-Host "===== EXCLUDING UNITY FOLDERS FROM ONEDRIVE =====" -ForegroundColor Cyan

$foldersToExclude = @("Library", "Temp", "obj", "Build", "Logs")

foreach ($folder in $foldersToExclude) {
    if (Test-Path $folder) {
        Write-Host "Excluding $folder from OneDrive..." -ForegroundColor Yellow
        attrib +U "$folder" /S /D
        Write-Host "$folder excluded!" -ForegroundColor Green
    }
}

Write-Host "`n===== DONE! =====" -ForegroundColor Green
Write-Host "OneDrive will no longer sync these folders." -ForegroundColor Cyan
Write-Host "This prevents the '1000 items' popup!" -ForegroundColor Green
