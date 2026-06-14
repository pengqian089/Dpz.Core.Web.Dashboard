param(
    [ValidateSet("prod", "dev", "build", "typecheck", "lint", "format", "format-check", "check", "clean")]
    [string]$Mode = "prod"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$clientAppPath = Join-Path $projectRoot "ClientApp"
$projectPath = Join-Path $projectRoot "Dpz.Core.Web.Dashboard.csproj"
$indexHtmlPath = Join-Path $projectRoot "wwwroot/index.html"
$assetsPath = Join-Path $projectRoot "wwwroot/assets"
$nodeCommand = Get-Command node.exe -ErrorAction SilentlyContinue
if ($null -eq $nodeCommand) {
    $nodeCommand = Get-Command node -ErrorAction Stop
}
$nodePath = $nodeCommand.Source

$npmCommand = Get-Command npm.cmd -ErrorAction SilentlyContinue
if ($null -eq $npmCommand) {
    $npmCommand = Get-Command npm -ErrorAction Stop
}
$npmPath = $npmCommand.Source

function Write-Step($message) {
    Write-Host "--------------------------------" -ForegroundColor Yellow
    Write-Host $message -ForegroundColor Yellow
    Write-Host "--------------------------------" -ForegroundColor Yellow
}

function Get-ProjectVersion {
    [xml]$csproj = Get-Content $projectPath
    return $csproj.Project.PropertyGroup.Version
}

function Invoke-Process {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [string[]]$ArgumentList = @()
    )

    $process = Start-Process `
        -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -NoNewWindow `
        -Wait `
        -PassThru

    if ($process.ExitCode -ne 0) {
        throw "$FilePath $($ArgumentList -join ' ') failed with exit code $($process.ExitCode)"
    }
}

function Remove-DirectoryInsideProject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $root = (Resolve-Path -LiteralPath $projectRoot).Path
    if (-not (Test-Path $Path)) {
        return
    }

    $target = (Resolve-Path -LiteralPath $Path).Path
    if (-not $target.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete outside project: $target"
    }

    Remove-Item -LiteralPath $target -Recurse -Force
}

function Clear-BuildArtifacts {
    Write-Step "Clean build artifacts"
    Remove-DirectoryInsideProject $assetsPath
    Remove-DirectoryInsideProject (Join-Path $projectRoot "bin")
    Remove-DirectoryInsideProject (Join-Path $projectRoot "obj")
}

function Ensure-NpmDependencies {
    $nodeModulesPath = Join-Path $clientAppPath "node_modules"
    if (Test-Path $nodeModulesPath) {
        return
    }

    Write-Step "Install frontend dependencies"
    Invoke-Process $npmPath $clientAppPath @("install")
}

function Invoke-NpmScript {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ScriptName
    )

    Write-Step "npm run $ScriptName"
    Invoke-Process $npmPath $clientAppPath @("run", $ScriptName)
}

function Invoke-ViteBuild {
    $vitePath = Join-Path $clientAppPath "node_modules/vite/bin/vite.js"
    if (-not (Test-Path $vitePath)) {
        throw "Vite entry was not found: $vitePath"
    }

    Write-Step "vite build"
    Push-Location $clientAppPath
    try {
        & $nodePath $vitePath build
        if ($LASTEXITCODE -ne 0) {
            throw "vite build failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-ViteWatch {
    $vitePath = Join-Path $clientAppPath "node_modules/vite/bin/vite.js"
    if (-not (Test-Path $vitePath)) {
        throw "Vite entry was not found: $vitePath"
    }

    Write-Step "vite build --watch"
    Push-Location $clientAppPath
    try {
        & $nodePath $vitePath build --watch
        if ($LASTEXITCODE -ne 0) {
            throw "vite build --watch failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Get-ManifestEntry {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Manifest,

        [Parameter(Mandatory = $true)]
        [string]$EntryName
    )

    $property = $Manifest.PSObject.Properties[$EntryName]
    if ($null -eq $property) {
        throw "Vite manifest entry '$EntryName' was not found."
    }

    return $property.Value
}

function Update-IndexHtml {
    $version = Get-ProjectVersion
    $manifestPath = Join-Path $assetsPath "manifest.json"
    if (-not (Test-Path $manifestPath)) {
        throw "Vite manifest was not found: $manifestPath"
    }

    $manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $appEntry = Get-ManifestEntry $manifest "src/app.ts"
    $appScriptPath = "assets/$($appEntry.file)"
    $appStylePath = "assets/$($appEntry.css[0])"
    $content = Get-Content $indexHtmlPath -Raw -Encoding UTF8

    $content = $content -replace `
        '<link data-vite-entry="app" href="[^"]+" rel="stylesheet" />',
        "<link data-vite-entry=`"app`" href=`"$appStylePath`" rel=`"stylesheet`" />"

    $content = $content -replace `
        '<script data-vite-entry="app" type="module" src="[^"]+"></script>',
        "<script data-vite-entry=`"app`" type=`"module`" src=`"$appScriptPath`"></script>"

    $content = $content -replace `
        '(<div class="app-loading__version">)v[\d\.]+(</div>)',
        "`${1}v$version`${2}"

    Set-Content -Path $indexHtmlPath -Value $content -NoNewline -Encoding UTF8
    Write-Host "index.html synced to v$version and Vite manifest assets" -ForegroundColor Green
}

Ensure-NpmDependencies

switch ($Mode) {
    "clean" {
        Clear-BuildArtifacts
        exit 0
    }
    "dev" {
        Invoke-ViteWatch
        exit 0
    }
    "typecheck" {
        Invoke-NpmScript "typecheck"
        exit 0
    }
    "lint" {
        Invoke-NpmScript "lint"
        exit 0
    }
    "format" {
        Invoke-NpmScript "format"
        exit 0
    }
    "format-check" {
        Invoke-NpmScript "format:check"
        exit 0
    }
    "check" {
        Invoke-NpmScript "check"
        exit 0
    }
    default {
        Clear-BuildArtifacts
        Invoke-ViteBuild
        Update-IndexHtml

        Write-Step "Build Blazor project"
        dotnet build $projectPath
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }
}
