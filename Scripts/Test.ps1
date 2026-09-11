param([Parameter(Mandatory=$true)][string]$EngineRoot)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$editor = Join-Path $EngineRoot 'Engine\Binaries\Win64\UnrealEditor-Cmd.exe'
if (-not (Test-Path -LiteralPath $editor)) { throw "UnrealEditor-Cmd.exe not found" }
& $editor (Join-Path $projectRoot 'ProjectDuskline.uproject') -unattended -nop4 -NullRHI '-ExecCmds=Automation RunTests Duskline' '-TestExit=Automation Test Queue Empty' "-ReportExportPath=$projectRoot\Saved\Automation" -log
if ($LASTEXITCODE -ne 0) { throw "Automation process failed: $LASTEXITCODE" }
Write-Host 'Inspect Saved/Automation/index.json for all test results; process exit alone is not a pass.'
