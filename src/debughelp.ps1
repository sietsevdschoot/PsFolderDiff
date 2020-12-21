[CmdLetBinding()]
param()

.\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

. ([ScriptBlock]::Create("using module $PSScriptRoot\BasicFileInfo.psm1"))
. ([ScriptBlock]::Create("using module $PSScriptRoot\FileHashLookup.Impl.psm1"))

