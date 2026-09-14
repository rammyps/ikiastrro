<#
.SYNOPSIS
    Clears every generated/user row in ikiastrro (tbl_Fact_*, tbl_Chart_*, tbl_ChartResults,
    tbl_BirthDetails) and reseeds their IDENTITY columns back to 0, so the next insert starts
    fresh at 1. Reference/seed data (tbl_Dim_*, tbl_Rule_*) and all table structures are left
    untouched.

.PARAMETER Server
    SQL Server instance. Defaults to the dev instance from SqlConnectionFactory.cs.

.PARAMETER Database
    Database name. Defaults to ikiastrro.

.PARAMETER Confirm
    Pass -Confirm:$false to skip the interactive y/n prompt (e.g. for CI/automation).

.EXAMPLE
    .\Reset-AllTransactionalData.ps1
    .\Reset-AllTransactionalData.ps1 -Confirm:$false
#>
[CmdletBinding(SupportsShouldProcess = $false)]
param(
    [string]$Server = 'localhost\SQLSERVER2025',
    [string]$Database = 'ikiastrro',
    [bool]$Confirm = $true
)

$ErrorActionPreference = 'Stop'
$sqlFile = Join-Path $PSScriptRoot 'reset_all_transactional_data.sql'

if (-not (Test-Path $sqlFile)) {
    throw "Reset script not found: $sqlFile"
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd not found on PATH. Install the SQL Server command-line tools (mssql-tools / sqlcmd)."
}

Write-Host "About to clear ALL people, chart results, chart analytics, and fact data in [$Database] on [$Server]." -ForegroundColor Yellow
Write-Host "Reference/rule/dimension seed tables are NOT touched." -ForegroundColor Yellow

if ($Confirm) {
    $answer = Read-Host "Type YES to proceed"
    if ($answer -ne 'YES') {
        Write-Host "Aborted. No changes made." -ForegroundColor Cyan
        exit 1
    }
}

& sqlcmd -S $Server -d $Database -E -b -i $sqlFile
if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd exited with code $LASTEXITCODE — reset did not complete cleanly."
}

Write-Host "Reset complete. Verifying..." -ForegroundColor Green

$verifyQuery = @"
SET NOCOUNT ON;
SELECT t.name AS TableName, p.rows AS RowCnt
FROM sys.tables t
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
WHERE t.name LIKE 'tbl_Fact_%' OR t.name LIKE 'tbl_Chart_%'
   OR t.name IN ('tbl_ChartResults','tbl_BirthDetails')
ORDER BY t.name;
"@

& sqlcmd -S $Server -d $Database -E -h -1 -W -s '|' -Q $verifyQuery
