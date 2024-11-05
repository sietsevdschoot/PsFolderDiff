[CmdLetBinding()]
param()

& $PSScriptRoot\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

& import-module (Join-Path $PSScriptRoot ..\src\MyFileHashLookup.psm1) -Force -Verbose

Get-FileHashTable $PSScriptRoot

# & (Join-Path $PSScriptRoot "\..\tests\CheckNugetDependencies.ps1") -Verify
