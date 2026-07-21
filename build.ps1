[CmdletBinding()]
param(
    [ValidateSet("run", "build", "publish", "installer")]
    [string]$Target = "publish",

    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$Root = $PSScriptRoot
$Project = Join-Path $Root "src\EditorDeClipboard\EditorDeClipboard.csproj"
$PublishDir = Join-Path $Root "publish"
$DistDir = Join-Path $Root "dist"
$Runtime = "win-x64"

function Get-AppVersion {
    $versionNode = Select-Xml `
        -Path $Project `
        -XPath "/*[local-name()='Project']/*[local-name()='PropertyGroup']/*[local-name()='Version']" |
        Select-Object -First 1

    if ($versionNode) {
        return $versionNode.Node.InnerText
    }

    return "0.0.0"
}

function Get-IsccPath {
    $command = Get-Command iscc -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    return $null
}

function Publish-App {
    $selfContainedValue = if ($SelfContained) { "true" } else { "false" }

    dotnet publish $Project `
        --configuration Release `
        --runtime $Runtime `
        --self-contained $selfContainedValue `
        -p:PublishSingleFile=true `
        -p:DebugType=none `
        -p:DebugSymbols=false `
        --output $PublishDir

    $exe = Join-Path $PublishDir "EditorDeClipboard.exe"
    $sizeKb = [Math]::Round((Get-Item -LiteralPath $exe).Length / 1KB, 2)

    Write-Host ""
    Write-Host "Executavel gerado:" -ForegroundColor Green
    Write-Host $exe
    Write-Host "Tamanho: $sizeKb KB"
}

Push-Location $Root

try {
    switch ($Target) {
        "run" {
            dotnet run --project $Project
        }

        "build" {
            dotnet build $Project --configuration Release
        }

        "publish" {
            Publish-App
        }

        "installer" {
            Publish-App

            $iscc = Get-IsccPath
            if (-not $iscc) {
                throw "Inno Setup nao encontrado. Instale pelo site oficial ou rode: choco install innosetup --yes"
            }

            New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

            $version = Get-AppVersion
            Push-Location (Join-Path $Root "installer")
            try {
                & $iscc "/DAppVersion=$version" "EditorDeClipboard.iss"
            }
            finally {
                Pop-Location
            }

            $installer = Get-ChildItem -Path $DistDir -Filter "*Setup*.exe" |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

            if ($installer) {
                $sizeMb = [Math]::Round($installer.Length / 1MB, 2)
                Write-Host ""
                Write-Host "Instalador gerado:" -ForegroundColor Green
                Write-Host $installer.FullName
                Write-Host "Tamanho: $sizeMb MB"
            }
        }
    }
}
finally {
    Pop-Location
}
