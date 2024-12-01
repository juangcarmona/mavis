$projectName = "MAVIS"

$installDir = [System.IO.Path]::Combine($env:USERPROFILE, $projectName)

if (-Not (Test-Path -Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force
}

dotnet publish "../src/MAVIS/MAVIS.csproj" --configuration Debug --output $installDir

if (Test-Path -Path "../appsettings.json") {
    Copy-Item -Path "../appsettings.json" -Destination $installDir -Force
}

if (Get-Alias mavis -ErrorAction SilentlyContinue) {
    Remove-Item Alias:mavis
}

$envPath = [System.Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::User)
$newPath = ($envPath -split ';') -notmatch [Regex]::Escape("$env:USERPROFILE\scripts")
[System.Environment]::SetEnvironmentVariable("Path", ($newPath -join ';'), [System.EnvironmentVariableTarget]::User)

$scriptContent = 'dotnet "%USERPROFILE%\MAVIS\MAVIS.dll" %*'
$scriptPath = [System.IO.Path]::Combine($env:USERPROFILE, 'scripts\mavis.cmd')
if (-Not (Test-Path -Path "$env:USERPROFILE\scripts")) {
    New-Item -ItemType Directory -Path "$env:USERPROFILE\scripts" -Force
}
$scriptContent | Out-File -FilePath $scriptPath -Encoding ASCII

$envPath = [System.Environment]::GetEnvironmentVariable("Path", [System.EnvironmentVariableTarget]::User)
if ($envPath -notlike "*$env:USERPROFILE\scripts*") {
    [System.Environment]::SetEnvironmentVariable("Path", "$envPath;$env:USERPROFILE\scripts", [System.EnvironmentVariableTarget]::User)
}

Write-Host "Installation completed. You can now use the 'mavis' command from any location. Please restart your terminal for the changes to take effect."
