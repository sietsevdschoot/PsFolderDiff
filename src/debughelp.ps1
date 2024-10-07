[CmdLetBinding()]
param()

& $PSScriptRoot\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

 . ([ScriptBlock]::Create("using module $PSScriptRoot\..\src\BasicFileInfo.psm1"))
 . ([ScriptBlock]::Create("using module $PSScriptRoot\..\src\FileHashLookup.Impl.psm1"))

 