#Requires -Version 7

[CmdletBinding()]
param ()

. ([ScriptBlock]::Create("using module $PSScriptRoot\BasicFileInfo.psm1"))
. ([ScriptBlock]::Create("using module $PSScriptRoot\FileHashLookup.Impl.psm1"))

$modules = Get-ChildItem $PSScriptRoot\*.psm1 -Recurse

foreach ($module in $modules) {

    Remove-Module $module.BaseName -Force -ErrorAction SilentlyContinue
    
    $output = Import-Module $module -Force -Verbose:($VerbosePreference -eq 'Continue') 4>&1

    $output | Select-String "Importing|Loading"
}