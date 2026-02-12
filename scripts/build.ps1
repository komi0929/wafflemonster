# ============================================
# SOYYA WebGL ビルド自動化スクリプト
# ============================================
# 使用法: .\scripts\build.ps1 [-Dev] [-Serve] [-Commit]
# ============================================

param(
    [switch]$Dev,      # 開発ビルド（高速、圧縮なし）
    [switch]$Serve,    # ビルド後にプレビューサーバー起動
    [switch]$Commit    # ビルド成功時にGit自動コミット
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Definition)

# ===== Unity Editor パス検出 =====
function Find-UnityEditor {
    $hubEditors = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path $hubEditors) {
        $latest = Get-ChildItem $hubEditors | Sort-Object Name -Descending | Select-Object -First 1
        $unityExe = Join-Path $latest.FullName "Editor\Unity.exe"
        if (Test-Path $unityExe) { return $unityExe }
    }

    # レジストリから検索
    $regPath = "HKLM:\SOFTWARE\Unity Technologies\Installs"
    if (Test-Path $regPath) {
        $installations = Get-ChildItem $regPath
        foreach ($install in $installations) {
            $path = (Get-ItemProperty $install.PSPath)."(default)"
            if ($path -and (Test-Path "$path\Unity.exe")) {
                return "$path\Unity.exe"
            }
        }
    }

    Write-Error "Unity Editor が見つかりません。Unity Hub でインストールしてください。"
    exit 1
}

# ===== メイン処理 =====
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  SOYYA WebGL ビルドパイプライン" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$unityPath = Find-UnityEditor
Write-Host "[INFO] Unity Editor: $unityPath" -ForegroundColor Green

$buildMethod = if ($Dev) { "Soyya.Editor.WebGLBuilder.BuildDev" } else { "Soyya.Editor.WebGLBuilder.Build" }
$buildType = if ($Dev) { "開発" } else { "本番" }
$logFile = Join-Path $ProjectRoot "Logs\webgl-build.log"

Write-Host "[INFO] ビルドタイプ: $buildType" -ForegroundColor Yellow
Write-Host "[INFO] 実行メソッド: $buildMethod" -ForegroundColor Yellow
Write-Host ""

# ===== ビルド実行 =====
$buildArgs = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $ProjectRoot,
    "-executeMethod", $buildMethod,
    "-logFile", $logFile,
    "-quit"
)

Write-Host "[BUILD] ビルド開始..." -ForegroundColor Cyan
$startTime = Get-Date

$process = Start-Process -FilePath $unityPath -ArgumentList $buildArgs -Wait -PassThru -NoNewWindow
$elapsed = (Get-Date) - $startTime

if ($process.ExitCode -eq 0) {
    Write-Host ""
    Write-Host "[SUCCESS] ✅ ビルド成功! (${elapsed.TotalSeconds}秒)" -ForegroundColor Green

    # ビルドサイズ表示
    $buildDir = if ($Dev) { "WebGL-Build-Dev" } else { "WebGL-Build" }
    $buildPath = Join-Path $ProjectRoot $buildDir
    if (Test-Path $buildPath) {
        $size = (Get-ChildItem $buildPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "[INFO] ビルドサイズ: $([math]::Round($size, 2)) MB" -ForegroundColor Green
    }

    # ===== Git自動コミット =====
    if ($Commit) {
        Write-Host ""
        Write-Host "[GIT] 自動コミット..." -ForegroundColor Yellow
        Push-Location $ProjectRoot
        git add -A
        $commitMsg = "build: WebGL ${buildType}ビルド成功 (${([math]::Round($elapsed.TotalSeconds))}秒)"
        git commit -m $commitMsg
        git push
        Pop-Location
        Write-Host "[GIT] ✅ コミット＆プッシュ完了" -ForegroundColor Green
    }

    # ===== プレビューサーバー起動 =====
    if ($Serve) {
        Write-Host ""
        Write-Host "[SERVE] プレビューサーバー起動..." -ForegroundColor Cyan
        $serveScript = Join-Path $ProjectRoot "scripts\serve.js"
        Start-Process -FilePath "node" -ArgumentList $serveScript -NoNewWindow
        Write-Host "[SERVE] http://localhost:8080 でプレビュー中" -ForegroundColor Green
    }

} else {
    Write-Host ""
    Write-Host "[FAILED] ❌ ビルド失敗 (Exit Code: $($process.ExitCode))" -ForegroundColor Red
    if (Test-Path $logFile) {
        Write-Host "[LOG] エラーログ (最後の20行):" -ForegroundColor Red
        Get-Content $logFile -Tail 20
    }
    exit 1
}
