param(
    [ValidateSet("prod", "dev", "build", "frontend", "typecheck", "lint", "format", "format-check", "check", "clean")]
    [string]$Mode = "prod",

    # "frontend" 模式只生成前端资产（npm install + Vite + 同步 index.html），
    # 不调用 dotnet build。MSBuild 通过它把前端编译接入构建图。
    # 此时 $projectRoot 必须由 -ProjectRoot 显式传入，因为
    # $MyInvocation.MyCommand.Path 在 `pwsh -File` 下不可靠。
    [string]$ProjectRoot = "",

    # 增量标记文件。它的 mtime 晚于所有前端源码时跳过 Vite。
    # 由 MSBuild 传入（放在应用项目的 obj/ 下）。
    [string]$StampFile = ""
)

$ErrorActionPreference = "Stop"

# Vite 输出包含 ✓ / ⚠ 等非 ASCII 字符。被 MSBuild Exec 调用时，
# 控制台编码不匹配会出现乱码，这里显式统一为 UTF-8。
try {
    [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding $false
}
catch {
}

# 关闭彩色与装饰性输出。本脚本会被 MSBuild Exec 以文本方式捕获输出，
# 此时 ANSI 转义序列与 ✓ 之类字符会显示为乱码；纯 ASCII 更易读。
# 对所有子进程（node / npm / dotnet）统一生效。
$env:NO_COLOR = "1"

$scriptRoot = if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    Split-Path -Parent $MyInvocation.MyCommand.Path
}
else {
    (Resolve-Path -LiteralPath $ProjectRoot).Path
}

$projectRoot = $scriptRoot
$clientAppPath = Join-Path (Split-Path -Parent $projectRoot) "Dpz.Core.Web.Management.Frontend"
$projectPath = Join-Path $projectRoot "Dpz.Core.Web.Management.csproj"
$indexHtmlPath = Join-Path $projectRoot "wwwroot/index.html"

# Vite 产物落在应用项目自己的 wwwroot/assets：静态 web 资产是由本项目从
# Content 通配符发现的，放到兄弟项目去是发现不到的（publish 会漏）。
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

$dotnetCommand = Get-Command dotnet -ErrorAction Stop
$dotnetPath = $dotnetCommand.Source

function Write-ConsoleMessage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [ConsoleColor]$ForegroundColor = [ConsoleColor]::White
    )

    try {
        Write-Host $Message -ForegroundColor $ForegroundColor
    }
    catch {
        try {
            [Console]::Out.WriteLine($Message)
        }
        catch {
        }
    }
}

function Write-Step($message) {
    Write-ConsoleMessage "--------------------------------" Yellow
    Write-ConsoleMessage $message Yellow
    Write-ConsoleMessage "--------------------------------" Yellow
}

function Get-ProjectVersion {
    # 版本号集中在仓库根的 Directory.Build.props，项目文件不再声明；两处都查以兼容。
    $propsFile = Join-Path (Split-Path -Parent (Split-Path -Parent $projectRoot)) "Directory.Build.props"
    foreach ($candidate in @($projectPath, $propsFile)) {
        if (-not (Test-Path $candidate)) {
            continue
        }

        [xml]$xml = Get-Content $candidate
        $version = $xml.Project.PropertyGroup.Version
        if (-not [string]::IsNullOrWhiteSpace($version)) {
            return $version
        }
    }

    throw "Version was not found in $projectPath or $propsFile."
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

    if (-not (Test-Path $Path)) {
        return
    }

    $root = (Resolve-Path -LiteralPath $projectRoot).Path
    $target = (Resolve-Path -LiteralPath $Path).Path
    if (-not $target.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete outside $root : $target"
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

function Invoke-Vite {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList
    )

    $vitePath = Join-Path $clientAppPath "node_modules/vite/bin/vite.js"
    if (-not (Test-Path $vitePath)) {
        throw "Vite entry was not found: $vitePath"
    }

    Write-Step "vite $($ArgumentList -join ' ')"
    Push-Location $clientAppPath
    try {
        # index.html 的同步由 viteIndexSyncPlugin 负责（见 sync-index.mjs），
        # 它是唯一实现；此处只需告知目标路径与版本号，未设置时插件自动跳过。
        $env:MSBUILD_INDEX_HTML = $indexHtmlPath
        $env:MSBUILD_VERSION = Get-ProjectVersion
        try {
            & $nodePath $vitePath @ArgumentList
            if ($LASTEXITCODE -ne 0) {
                throw "vite $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE"
            }
        }
        finally {
            Remove-Item Env:\MSBUILD_INDEX_HTML -ErrorAction SilentlyContinue
            Remove-Item Env:\MSBUILD_VERSION -ErrorAction SilentlyContinue
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-ViteBuild {
    # --logLevel warn：Vite 默认会打印每个产物的体积表与 ✓ 汇总行。
    # 接入 dotnet build 后这些属于噪音，且 ✓ 在 MSBuild 捕获输出时会因编码不匹配显示为乱码。
    Invoke-Vite @("build", "--logLevel", "warn")
}

function Start-DevProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Label,

        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList
    )

    Write-Step $Label
    return Start-Process `
        -FilePath $FilePath `
        -ArgumentList $ArgumentList `
        -WorkingDirectory $WorkingDirectory `
        -NoNewWindow `
        -PassThru
}

function Stop-ProcessTree {
    param(
        [System.Diagnostics.Process]$Process
    )

    if ($null -eq $Process) {
        return
    }

    $processId = $Process.Id
    try {
        $children = Get-CimInstance Win32_Process -Filter "ParentProcessId = $processId"
        foreach ($child in $children) {
            try {
                $childProcess = Get-Process -Id $child.ProcessId -ErrorAction SilentlyContinue
                Stop-ProcessTree $childProcess
            }
            catch {
                Write-Warning "Failed to stop child process $($child.ProcessId): $($_.Exception.Message)"
            }
        }

        $currentProcess = Get-Process -Id $processId -ErrorAction SilentlyContinue
        if ($null -ne $currentProcess -and -not $currentProcess.HasExited) {
            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        }
    }
    catch {
        Write-Warning "Failed to stop process ${processId}: $($_.Exception.Message)"
    }
}

function Invoke-DevMode {
    Invoke-ViteBuild

    $viteProcess = $null
    $dotnetProcess = $null
    $script:stopDevMode = $false
    $cancelHandler = [ConsoleCancelEventHandler] {
        param($sender, $eventArgs)

        $eventArgs.Cancel = $true
        $script:stopDevMode = $true
    }

    [Console]::add_CancelKeyPress($cancelHandler)

    try {
        # Vite 以 --watch 常驻，index.html 的每次重同步由插件在同一进程内完成，
        # 因此不再需要轮询 manifest 的 mtime。
        $env:MSBUILD_INDEX_HTML = $indexHtmlPath
        $env:MSBUILD_VERSION = Get-ProjectVersion
        $viteProcess = Start-DevProcess `
            "vite build --watch" `
            $nodePath `
            $clientAppPath `
            @($vitePath, "build", "--watch", "--logLevel", "warn")

        $dotnetProcess = Start-DevProcess `
            "dotnet watch run" `
            $dotnetPath `
            $projectRoot `
            @("watch", "--project", $projectPath, "run")

        Write-ConsoleMessage "Dev mode is running. Press Ctrl+C to stop Vite and dotnet watch." Green

        while (-not $script:stopDevMode) {
            if ($viteProcess.HasExited) {
                throw "vite build --watch exited with code $($viteProcess.ExitCode)"
            }

            if ($dotnetProcess.HasExited) {
                throw "dotnet watch run exited with code $($dotnetProcess.ExitCode)"
            }

            Start-Sleep -Milliseconds 500
        }
    }
    finally {
        [Console]::remove_CancelKeyPress($cancelHandler)
        Write-Step "Stop dev processes"
        Stop-ProcessTree $viteProcess
        Stop-ProcessTree $dotnetProcess
    }
}

function Test-FrontendUpToDate {
    if ([string]::IsNullOrWhiteSpace($StampFile)) {
        return $false
    }

    if (-not (Test-Path $StampFile)) {
        return $false
    }

    # Vite 生成的入口文件名带 content hash，写一次就不会变，
    # 因此“标记晚于全部前端源码”即可判定无需重建。
    $stampTime = (Get-Item -LiteralPath $StampFile).LastWriteTimeUtc
    $sourceRoot = Join-Path $clientAppPath "src"
    if (-not (Test-Path $sourceRoot)) {
        return $false
    }

    $newest = Get-ChildItem -LiteralPath $sourceRoot -Recurse -File |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if ($null -eq $newest) {
        return $false
    }

    return $stampTime -gt $newest.LastWriteTimeUtc
}

function Write-FrontendStamp {
    if ([string]::IsNullOrWhiteSpace($StampFile)) {
        return
    }

    $parent = Split-Path -Parent $StampFile
    if (-not (Test-Path $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    Set-Content -LiteralPath $StampFile -Value "" -Encoding ascii -NoNewline
}

Ensure-NpmDependencies

switch ($Mode) {
    "clean" {
        Clear-BuildArtifacts
        if (-not [string]::IsNullOrWhiteSpace($StampFile) -and (Test-Path $StampFile)) {
            Remove-Item -LiteralPath $StampFile -Force
        }

        exit 0
    }
    "frontend" {
        if (Test-FrontendUpToDate) {
            Write-ConsoleMessage "Frontend assets are up to date" Green
            exit 0
        }

        Invoke-ViteBuild
        Write-FrontendStamp
        exit 0
    }
    "dev" {
        Invoke-DevMode
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

        Write-Step "Build Blazor project"
        dotnet build $projectPath
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }
}
