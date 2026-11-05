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
    [xml]$csproj = Get-Content $projectPath
    return $csproj.Project.PropertyGroup.Version
}

function Set-Utf8NoBomContent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $encoding = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
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

    # 该函数通常在 Ctrl+C 之后的 finally 中运行，此时管道可能已经处于停止状态，
    # 因此每一步都要吞掉异常，避免清理过程中再次抛错导致子进程残留。
    # 优先用 taskkill /T 一次性结束整棵进程树：不依赖 WMI，比 Get-CimInstance 更快。
    try {
        & taskkill.exe /PID $processId /T /F *> $null
    }
    catch {
    }

    if ($null -eq (Get-Process -Id $processId -ErrorAction SilentlyContinue)) {
        return
    }

    # 退回方案：taskkill 不可用或被策略拦截时，改为递归结束子进程（依赖 WMI）。
    try {
        $children = @(
            Get-CimInstance Win32_Process `
                -Filter "ParentProcessId = $processId" `
                -ErrorAction SilentlyContinue
        )

        foreach ($child in $children) {
            $childProcess = Get-Process -Id $child.ProcessId -ErrorAction SilentlyContinue
            if ($null -ne $childProcess) {
                Stop-ProcessTree $childProcess
            }
        }
    }
    catch {
    }

    try {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    }
    catch {
    }
}

function Stop-DevProcesses {
    param(
        [System.Diagnostics.Process]$ViteProcess,

        [System.Diagnostics.Process]$DotnetProcess
    )

    Write-Step "Stop dev processes"
    Stop-ProcessTree $ViteProcess
    Stop-ProcessTree $DotnetProcess
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

    Set-Utf8NoBomContent $indexHtmlPath $content
    Write-ConsoleMessage "index.html synced to v$version and Vite manifest assets" Green
}

function Get-ManifestLastWriteTimeUtc {
    $manifestPath = Join-Path $assetsPath "manifest.json"
    if (-not (Test-Path $manifestPath)) {
        return $null
    }

    return (Get-Item -LiteralPath $manifestPath).LastWriteTimeUtc
}

function Invoke-DevMode {
    $vitePath = Join-Path $clientAppPath "node_modules/vite/bin/vite.js"
    if (-not (Test-Path $vitePath)) {
        throw "Vite entry was not found: $vitePath"
    }

    Invoke-ViteBuild
    Update-IndexHtml

    $viteProcess = $null
    $dotnetProcess = $null

    # 这里刻意不注册 [Console]::CancelKeyPress 的脚本块回调：
    # 该回调运行在控制台输入线程上，而此线程无权执行 PowerShell 代码（主线程此时
    # 正占用着管道），回调里的赋值会抛出未捕获的异常，最终以 0xE0434352 让整个
    # pwsh 进程崩溃（即 "pwsh.exe - 应用程序错误" 弹窗）。
    # 现在改为依赖 PowerShell 默认的 Ctrl+C 行为：中断当前管道并执行 finally。
    # 子进程由 -NoNewWindow 与当前控制台共享，因而同样会收到 Ctrl+C 并自行退出。
    try {
        $lastManifestWriteTime = Get-ManifestLastWriteTimeUtc
        $viteProcess = Start-DevProcess `
            "vite build --watch" `
            $nodePath `
            $clientAppPath `
            @($vitePath, "build", "--watch")

        $dotnetProcess = Start-DevProcess `
            "dotnet watch run" `
            $dotnetPath `
            $projectRoot `
            @("watch", "--project", $projectPath, "run")

        Write-ConsoleMessage "Dev mode is running. Press Ctrl+C to stop Vite and dotnet watch." Green

        while ($true) {
            if ($viteProcess.HasExited) {
                throw "vite build --watch exited with code $($viteProcess.ExitCode)"
            }

            if ($dotnetProcess.HasExited) {
                throw "dotnet watch run exited with code $($dotnetProcess.ExitCode)"
            }

            $manifestWriteTime = Get-ManifestLastWriteTimeUtc
            if ($null -ne $manifestWriteTime -and $manifestWriteTime -ne $lastManifestWriteTime) {
                try {
                    Update-IndexHtml
                    $lastManifestWriteTime = $manifestWriteTime
                }
                catch {
                    Write-ConsoleMessage `
                        "Failed to sync index.html: $($_.Exception.Message)" `
                        DarkYellow
                }
            }

            Start-Sleep -Milliseconds 500
        }
    }
    finally {
        Stop-DevProcesses $viteProcess $dotnetProcess
    }
}

Ensure-NpmDependencies

switch ($Mode) {
    "clean" {
        Clear-BuildArtifacts
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
        Update-IndexHtml

        Write-Step "Build Blazor project"
        dotnet build $projectPath
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE"
        }
    }
}
