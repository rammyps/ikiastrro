[CmdletBinding()]
param(
    [string]$ServerInstance = 'localhost\SQLSERVER2025',
    [string]$Database = 'ikiastrro',
    [string]$YogaDirectory = (Join-Path $PSScriptRoot '..\_research\Maitreya8\src\resources\yogas'),
    [byte]$RuleSetId = 0
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $YogaDirectory -PathType Container)) {
    throw "Maitreya yoga directory not found: $YogaDirectory"
}

$connectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=True;TrustServerCertificate=True"
$connection = [System.Data.SqlClient.SqlConnection]::new($connectionString)
$connection.Open()
$transaction = $connection.BeginTransaction()

function Invoke-ScalarQuery {
    param([string]$Sql)
    $command = $connection.CreateCommand()
    $command.Transaction = $transaction
    $command.CommandText = $Sql
    return $command.ExecuteScalar()
}

try {
    $systemId = [int](Invoke-ScalarQuery "SELECT Id FROM dbo.tbl_Dim_ValidationSystems WHERE Code='MAITREYA' AND ProductVersion='8.2';")
    if ($systemId -le 0) { throw 'Maitreya 8.2 validation system is not seeded.' }

    if ($RuleSetId -eq 0) {
        $RuleSetId = [byte](Invoke-ScalarQuery 'SELECT TOP (1) Id FROM dbo.tbl_Rule_Sets ORDER BY Id DESC;')
    }
    if ($RuleSetId -le 0) { throw 'No rule set is available for the import.' }

    $inserted = 0
    $unchanged = 0
    $files = Get-ChildItem -LiteralPath $YogaDirectory -Filter '*.json' -File | Sort-Object Name

    foreach ($file in $files) {
        $document = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
        $ordinal = 0
        foreach ($config in $document.configs) {
            $ordinal++
            $externalCode = 'MAITREYA82_{0}_{1:D4}' -f $file.BaseName.ToUpperInvariant(), $ordinal
            $compactJson = $config | ConvertTo-Json -Compress -Depth 20
            $hashBytes = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($compactJson))
            $definitionHash = [Convert]::ToHexString($hashBytes).ToLowerInvariant()
            $sourceLocator = 'https://github.com/martin-pe/maitreya8/blob/v8.2/src/resources/yogas/{0}#config-{1}' -f $file.Name, $ordinal

            $existingCommand = $connection.CreateCommand()
            $existingCommand.Transaction = $transaction
            $existingCommand.CommandText = @'
SELECT DefinitionHash
FROM dbo.tbl_Rule_YogaValidationDefinition
WHERE RuleSetId=@RuleSetId AND ValidationSystemId=@SystemId AND ExternalDefinitionCode=@Code;
'@
            [void]$existingCommand.Parameters.Add('@RuleSetId', [System.Data.SqlDbType]::TinyInt)
            [void]$existingCommand.Parameters.Add('@SystemId', [System.Data.SqlDbType]::Int)
            [void]$existingCommand.Parameters.Add('@Code', [System.Data.SqlDbType]::VarChar, 100)
            $existingCommand.Parameters['@RuleSetId'].Value = $RuleSetId
            $existingCommand.Parameters['@SystemId'].Value = $systemId
            $existingCommand.Parameters['@Code'].Value = $externalCode
            $existingHash = $existingCommand.ExecuteScalar()

            if ($null -ne $existingHash -and $existingHash -ne [DBNull]::Value) {
                if ([string]$existingHash -ne $definitionHash) {
                    throw "Definition changed without a new external code: $externalCode"
                }
                $unchanged++
                continue
            }

            $insertCommand = $connection.CreateCommand()
            $insertCommand.Transaction = $transaction
            $insertCommand.CommandText = @'
INSERT dbo.tbl_Rule_YogaValidationDefinition
    (RuleSetId, ValidationSystemId, ExternalDefinitionCode, ExternalName, SourceLocator,
     GroupName, ExpressionLanguage, ExpressionText, DefinitionHash, HigherVargasSupported)
OUTPUT INSERTED.Id
VALUES
    (@RuleSetId, @SystemId, @Code, @Name, @Locator, @GroupName,
     'MAITREYA_YOGAEXPERT', @Expression, @Hash, @HigherVargas);
'@
            [void]$insertCommand.Parameters.Add('@RuleSetId', [System.Data.SqlDbType]::TinyInt)
            [void]$insertCommand.Parameters.Add('@SystemId', [System.Data.SqlDbType]::Int)
            [void]$insertCommand.Parameters.Add('@Code', [System.Data.SqlDbType]::VarChar, 100)
            [void]$insertCommand.Parameters.Add('@Name', [System.Data.SqlDbType]::NVarChar, 300)
            [void]$insertCommand.Parameters.Add('@Locator', [System.Data.SqlDbType]::NVarChar, 500)
            [void]$insertCommand.Parameters.Add('@GroupName', [System.Data.SqlDbType]::NVarChar, 120)
            [void]$insertCommand.Parameters.Add('@Expression', [System.Data.SqlDbType]::NVarChar, -1)
            [void]$insertCommand.Parameters.Add('@Hash', [System.Data.SqlDbType]::Char, 64)
            [void]$insertCommand.Parameters.Add('@HigherVargas', [System.Data.SqlDbType]::Bit)
            $insertCommand.Parameters['@RuleSetId'].Value = $RuleSetId
            $insertCommand.Parameters['@SystemId'].Value = $systemId
            $insertCommand.Parameters['@Code'].Value = $externalCode
            $insertCommand.Parameters['@Name'].Value = ([string]$config.description).Substring(0, [Math]::Min(300, ([string]$config.description).Length))
            $insertCommand.Parameters['@Locator'].Value = $sourceLocator
            $insertCommand.Parameters['@GroupName'].Value = if ($null -eq $config.group) { [DBNull]::Value } else { [string]$config.group }
            $insertCommand.Parameters['@Expression'].Value = if ($null -eq $config.rule) { [DBNull]::Value } else { [string]$config.rule }
            $insertCommand.Parameters['@Hash'].Value = $definitionHash
            $insertCommand.Parameters['@HigherVargas'].Value = if ($null -eq $config.highervargas) { [DBNull]::Value } else { [bool]$config.highervargas }
            $definitionId = [int]$insertCommand.ExecuteScalar()

            $mappingCommand = $connection.CreateCommand()
            $mappingCommand.Transaction = $transaction
            $mappingCommand.CommandText = @'
INSERT dbo.tbl_Dim_YogaValidationMappings
    (YogaValidationDefinitionId, MappingStatus, ReviewStatus, Rationale)
VALUES (@DefinitionId, 'UNMAPPED', 'PENDING',
        N'Imported from Maitreya 8.2; canonical identity requires source-aware review.');
'@
            [void]$mappingCommand.Parameters.Add('@DefinitionId', [System.Data.SqlDbType]::Int)
            $mappingCommand.Parameters['@DefinitionId'].Value = $definitionId
            [void]$mappingCommand.ExecuteNonQuery()
            $inserted++
        }
    }

    $transaction.Commit()
    Write-Output "Maitreya 8.2 import complete: files=$($files.Count), inserted=$inserted, unchanged=$unchanged, ruleSetId=$RuleSetId"
}
catch {
    if ($transaction.Connection) { $transaction.Rollback() }
    throw
}
finally {
    $connection.Dispose()
}
