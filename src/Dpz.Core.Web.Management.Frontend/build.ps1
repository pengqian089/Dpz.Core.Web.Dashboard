# 前端项目内的便捷入口。
# 集成构建（Vite 产物 + 同步应用 index.html + dotnet build）的实现在应用项目，
# 因为它需要读写同级应用项目的 wwwroot。这里只做转发，避免逻辑重复。
#
# 只想跑前端 Vite 时，直接 npm run watch / npm run build 即可。

param(
    [ValidateSet("prod", "dev", "build", "typecheck", "lint", "format", "format-check", "check", "clean")]
    [string]$Mode = "prod"
)

$ErrorActionPreference = "Stop"

$frontendRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appBuildScript = Join-Path (Split-Path -Parent $frontendRoot) "Dpz.Core.Web.Management\build.ps1"

if (-not (Test-Path $appBuildScript)) {
    throw "Application build script was not found: $appBuildScript"
}

& $appBuildScript -Mode $Mode
exit $LASTEXITCODE
