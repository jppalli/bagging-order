param([string]$Project = 'bagging-order')
$ErrorActionPreference = 'Stop'
$build = Join-Path $PSScriptRoot 'UnityGame/Builds/WebGL'
if (!(Test-Path (Join-Path $build 'index.html'))) { throw 'Run ./build-web.ps1 first.' }
$loader = Get-Content (Join-Path $build 'Build/WebGL.loader.js') -Raw
if (!$loader.Contains('optional-worker-fix')) { throw 'Rebuild with the web loader fix before deploying.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'deploy/vercel.json') -Destination (Join-Path $build 'vercel.json')
Push-Location $build
try {
    & vercel deploy --prod --yes --name $Project
    if ($LASTEXITCODE -ne 0) { throw 'Vercel deployment failed.' }
} finally { Pop-Location }
