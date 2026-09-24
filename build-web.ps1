param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.0.39f1/Editor/Unity.exe')
$ErrorActionPreference = 'Stop'
if (!(Test-Path -LiteralPath $UnityEditor)) { throw "Unity Editor not found: $UnityEditor" }
$project = Join-Path $PSScriptRoot 'UnityGame'
$log = Join-Path $project 'build-web.log'
$arguments = '-batchmode -quit -buildTarget WebGL -projectPath "{0}" -executeMethod BuildGame.Web -baggingSkipChecks -logFile "{1}"' -f $project,$log
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0 -or !(Select-String -LiteralPath $log -Pattern 'BAGGING_WEB_BUILD_PASS' -Quiet)) {
    throw "Web build did not complete. See $log"
}
Write-Host "Web build ready: $project/Builds/WebGL"
