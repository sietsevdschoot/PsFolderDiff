[CmdLetBinding()]
param()

& $PSScriptRoot\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

# & (Join-Path $PSScriptRoot "\..\tests\CheckNugetDependencies.ps1") -GenerateNugetStatements -Verify

& import-module (Join-Path $PSScriptRoot ..\src\MyFileHashLookup.psm1) -Force -Verbose

$fileHashTable = Get-FileHashTable $PSScriptRoot

Get-Variable fileHashTable

