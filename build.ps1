<#
.SYNOPSIS
    Clean & Build Script for UNLowCoder solution
.DESCRIPTION
    - Löscht alle bin/obj-Ordner rekursiv
    - Baut Projekte in definierter Reihenfolge
    - Baut UNLowCoder.SourceGen zweimal (2. Lauf = Rebuild)
    - Stoppt bei erstem Fehler
#>

param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [switch]$NoClean
)

$ErrorActionPreference = 'Stop'

# Root-Ordner der Solution (Ordner des Skripts)
$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $root

function Write-Header($msg) {
    Write-Host "`n=== $msg ===" -ForegroundColor Cyan
}

function Clean-BuildDirs {
    Write-Header "Cleaning bin/obj directories..."
    Get-ChildItem -Path $root -Directory -Recurse -Force |
        Where-Object { $_.Name -in @('bin','obj') } |
        ForEach-Object {
            try {
                Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
                Write-Host "Removed $($_.FullName)" -ForegroundColor DarkGray
            } catch {
                Write-Warning "Could not remove $($_.FullName): $($_.Exception.Message)"
            }
        }
}

function Build-Project([string]$projPath) {
    Write-Header "Building $projPath ($Configuration)"
    dotnet build $projPath -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed for $projPath"
        exit $LASTEXITCODE
    }
}

function Rebuild-Project([string]$projPath) {
    Write-Header "Rebuilding $projPath ($Configuration)"
    dotnet build $projPath -c $Configuration -t:Rebuild
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Rebuild failed for $projPath"
        exit $LASTEXITCODE
    }
}

if (-not $NoClean) { Clean-BuildDirs }

# ---- Reihenfolge exakt wie gefordert ----------------------------------------

Build-Project   "UNLowCoder/UNLowCoder.Core.csproj"
Build-Project   "UNLowCoder.Extensions/UNLowCoder.Extensions.csproj"

# SourceGen: erst normal bauen ...
Build-Project   "UNLowCoder.SourceGen/UNLowCoder.SourceGen.csproj"
# ... dann explizit REBUILD
Rebuild-Project "UNLowCoder.SourceGen/UNLowCoder.SourceGen.csproj"

Build-Project   "UNLowCoder.Lib/UNLowCoder.Lib.csproj"

Write-Host "`n✅ Build completed successfully." -ForegroundColor Green
