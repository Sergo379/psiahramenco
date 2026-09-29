$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$outputDirectory = Join-Path $projectRoot 'dist\TicketStudio'
dotnet publish (Join-Path $projectRoot 'src\ExamTicketGenerator\ExamTicketGenerator.csproj') -p:PublishProfile=Portable -p:UseSharedCompilation=false -o $outputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Не удалось собрать приложение.' }
$demoProcess = Start-Process -FilePath (Join-Path $outputDirectory 'TicketStudio.exe') -ArgumentList '--create-demo' -WindowStyle Hidden -Wait -PassThru
if ($demoProcess.ExitCode -ne 0) { throw 'Не удалось подготовить примеры данных.' }
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs\ОТКРОЙ-МЕНЯ.txt') -Destination $outputDirectory
Write-Host "Готово: $outputDirectory\TicketStudio.exe"
