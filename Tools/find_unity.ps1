$candidates = @(
    'C:\Program Files\Unity\Hub\Editor',
    'C:\Program Files\Unity',
    'C:\Program Files (x86)\Unity',
    "$env:USERPROFILE\\AppData\\Local\\Programs\\Unity\\Hub\\Editor"
)
$found = ''
foreach ($p in $candidates) {
    if (Test-Path $p) {
        try {
            $u = Get-ChildItem -Path $p -Recurse -Filter Unity.exe -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($u) { $found = $u.FullName; break }
        } catch { }
    }
}
Write-Output $found
