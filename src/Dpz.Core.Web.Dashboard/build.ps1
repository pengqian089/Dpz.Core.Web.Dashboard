$location = Get-Location
$path = $location.Path

$cssSourcePath = [System.IO.Path]::Combine($path,'wwwroot','css')

# 收集 CSS 文件名（使用相对路径）
$cssFiles = @()

foreach($item in Get-ChildItem $cssSourcePath)
{
    if($item.Name -match 'global\.min\..+\.css(\.map)?$')
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
    # 删除旧的 global 文件
    Write-Host "Cleaning old global files..." -ForegroundColor cyan
    Get-ChildItem -Filter "global.min.*.css*" | Remove-Item -Force
    Write-Host "Old files removed." -ForegroundColor green
    
    $inputParameters = [System.String]::Join(" ",$cssFiles)
    $tempOutputFile = "global.min.temp.css"
    
    # 使用相对路径，source map 路径会更清晰
    $execute = "cleancss -o $tempOutputFile $inputParameters --with-rebase --source-map --debug"
    
    Write-Host "Executing: $execute" -ForegroundColor gray
    Invoke-Expression $execute
    
    # 计算文件的 hash
    $fileHash = Get-FileHash -Path $tempOutputFile -Algorithm MD5
    $hashString = $fileHash.Hash.Substring(0, 8).ToLower()
    
    # 重命名文件为带 hash 的文件名
    $outputFile = "global.min.$hashString.css"
    $outputMapFile = "global.min.$hashString.css.map"
    
    Rename-Item -Path $tempOutputFile -NewName $outputFile -Force
    
    # 如果有 source map 文件，也重命名它
    $tempMapFile = "$tempOutputFile.map"
    if (Test-Path $tempMapFile) {
        # 更新 source map 文件内容中的文件名引用
        $mapContent = Get-Content $tempMapFile -Raw
        $mapContent = $mapContent -replace [regex]::Escape($tempOutputFile), $outputFile
        Set-Content -Path $tempMapFile -Value $mapContent -NoNewline
        
        Rename-Item -Path $tempMapFile -NewName $outputMapFile -Force
        
        # 更新 CSS 文件中的 sourceMappingURL
        $cssContent = Get-Content $outputFile -Raw
        $cssContent = $cssContent -replace "sourceMappingURL=$([regex]::Escape($tempOutputFile))\.map", "sourceMappingURL=$outputMapFile"
        Set-Content -Path $outputFile -Value $cssContent -NoNewline
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor green
    Write-Host "  Output: wwwroot/css/$outputFile" -ForegroundColor green
    Write-Host "  Hash: $hashString" -ForegroundColor cyan
}
catch {
    Write-Host "Build failed: $_" -ForegroundColor red
    exit 1
}
finally {
    # 恢复原来的目录
    Pop-Location
}

# 更新 index.html 中的引用
$indexHtmlPath = [System.IO.Path]::Combine($path, 'wwwroot', 'index.html')
if (Test-Path $indexHtmlPath) {
    Write-Host "Updating index.html..." -ForegroundColor cyan
    
    $indexContent = Get-Content $indexHtmlPath -Raw -Encoding UTF8
    # 匹配任何 global.min 开头的 CSS 文件引用
    $pattern = '<link href="css/global\.min\.[^"]*\.css" rel="stylesheet" />'
    $replacement = '<link href="css/{0}" rel="stylesheet" />' -f $outputFile
    
    if ($indexContent -match $pattern) {
        $indexContent = $indexContent -replace $pattern, $replacement
        Set-Content -Path $indexHtmlPath -Value $indexContent -NoNewline -Encoding UTF8
        Write-Host "  index.html updated successfully!" -ForegroundColor green
    }
    else {
        # 如果找不到带 hash 的引用，尝试替换原始引用
        $pattern = '<link href="css/global\.min\.css" rel="stylesheet" />'
        if ($indexContent -match $pattern) {
            $indexContent = $indexContent -replace $pattern, $replacement
            Set-Content -Path $indexHtmlPath -Value $indexContent -NoNewline -Encoding UTF8
            Write-Host "  index.html updated successfully!" -ForegroundColor green
        }
        else {
            Write-Host "  Warning: Could not find global.min.css reference in index.html" -ForegroundColor yellow
        }
    }
}
else {
    Write-Host "  Warning: index.html not found" -ForegroundColor yellow
}