<#
.SYNOPSIS
    Force-recalculates every registered chart type + Vimshottari Dasha for every saved person
    (tbl_BirthDetails), even ones already built. Backs the "RECALCULATE" button on
    SavedCharts.razor. See docs/database/rebuild-generated-data-plan.md for the fuller design
    this is a lean slice of.

.DESCRIPTION
    Thin launcher: all calculation and persistence logic lives in ChartGenerationService via the
    `dotnet run --project src/Ikiastrro.Cli -- rebuild-all` command. This script does not touch
    SQL directly and does not duplicate any calculation logic.

.PARAMETER Confirm
    Pass -Confirm:$false to skip the interactive y/n prompt (e.g. for CI/automation, or when
    called from DatabaseMaintenanceService).

.EXAMPLE
    .\Rebuild-AllGeneratedData.ps1
    .\Rebuild-AllGeneratedData.ps1 -Confirm:$false
#>
[CmdletBinding(SupportsShouldProcess = $false)]
param(
    [bool]$Confirm = $true
)

$ErrorActionPreference = 'Stop'
$root = (git rev-parse --show-toplevel).Trim()
$cliProject = Join-Path $root 'src\Ikiastrro.Cli'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet not found on PATH."
}

Write-Host "About to force-recalculate ALL saved people's charts and Dasha (existing results are overwritten)." -ForegroundColor Yellow

if ($Confirm) {
    $answer = Read-Host "Type YES to proceed"
    if ($answer -ne 'YES') {
        Write-Host "Aborted. No changes made." -ForegroundColor Cyan
        exit 1
    }
}

& dotnet run --project $cliProject -- rebuild-all
if ($LASTEXITCODE -ne 0) {
    throw "rebuild-all exited with code $LASTEXITCODE — one or more people failed to rebuild."
}

Write-Host "Rebuild complete." -ForegroundColor Green
