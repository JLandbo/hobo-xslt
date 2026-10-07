$publish = Join-Path $PSScriptRoot "publish"
$exe = Join-Path $publish "HoboXslt.App.exe"
# Unsaved edits only live in the open app, so it is closed by the user instead of stopped.
while (Get-Process HoboXslt.App -ErrorAction SilentlyContinue | Where-Object Path -eq $exe) {
    Read-Host "hobo-xslt kører. Luk hobo-xslt, og tryk Enter"
}
dotnet publish "$PSScriptRoot\src\HoboXslt.App" -c Release -o $publish
if ($LASTEXITCODE -ne 0) { throw "Publish af hobo-xslt fejlede." }

$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut("$([Environment]::GetFolderPath('Programs'))\hobo-xslt.lnk")
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = $publish
$shortcut.Save()
Write-Host "Genvej oprettet i Start-menuen: $($shortcut.FullName)"
explorer.exe $exe