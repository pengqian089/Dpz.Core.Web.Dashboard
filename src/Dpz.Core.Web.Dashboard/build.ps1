$location = Get-Location
$path = $location.Path

$cssSourcePath = [System.IO.Path]::Combine($path,'wwwroot','css')

# 收集 CSS 文件名（使用相对路径）
$cssFiles = @()

foreach($item in Get-ChildItem $cssSourcePath)
{
    if($item.Name -match 'global.min.css|global.min.css.map')
    {
        continue;
    }
    # 只使用文件名，不使用完整路径
    $cssFiles += $item.Name
}

Write-Host "--------------------------------" -ForegroundColor yellow
Write-Host "Merge global style" -ForegroundColor yellow
Write-Host "--------------------------------" -ForegroundColor yellow
Write-Host "CSS Files: $($cssFiles.Count)" -ForegroundColor cyan

# 切换到 CSS 目录下执行，这样路径就是相对的
Push-Location $cssSourcePath

try {
    $inputParameters = [System.String]::Join(" ",$cssFiles)
    $outputFile = "global.min.css"
    
    # 使用相对路径，source map 路径会更清晰
    $execute = "cleancss -o $outputFile $inputParameters --with-rebase --source-map --debug"
    
    Write-Host "Executing: $execute" -ForegroundColor gray
    Invoke-Expression $execute
    
    Write-Host "Build completed successfully!" -ForegroundColor green
    Write-Host "  Output: wwwroot/css/$outputFile" -ForegroundColor green
}
catch {
    Write-Host "Build failed: $_" -ForegroundColor red
    exit 1
}
finally {
    # 恢复原来的目录
    Pop-Location
}