Clear-Host

Write-Host "Starting publish"
dotnet publish -r win10-arm -c Release

Write-Host "Starting Copy"
Copy-Item -Recurse .\bin\Release\netcoreapp2.0\win10-arm\ -Destination \\192.168.137.83\c$\Data\Users\Administrator\Documents
