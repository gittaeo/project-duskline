param([string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.2.0f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw "Unity Editor not found: $UnityEditor" }
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'Logs') | Out-Null
$arguments = @('-batchmode','-nographics','-quit','-projectPath',('"' + $projectRoot + '"'),'-executeMethod','Duskline.Editor.BuildTools.BuildWeb','-logFile',('"' + (Join-Path $projectRoot 'Logs/WebBuild.log') + '"'))
$buildProcess = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity Web build failed. See Logs/WebBuild.log" }
Get-Content -LiteralPath (Join-Path $projectRoot 'Validation/WebBuild.txt')
