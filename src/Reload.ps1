using module '.\FileHashLookup.Impl.psm1'
using module '.\BasicFileInfo.psm1'

[cmdletbinding()]
param (
    [switch] $unload
)

$customModules = Get-ChildItem $PSScriptRoot\*.psm1 -recurse -Exclude @("Pester.psm1")

$myVerbosePreference = $VerbosePreference

$VerbosePreference = "Silent"

if ($unload) {

    $customModules | Foreach-Object { Remove-Module ($_.Name -replace $_.Extension) -Force -ErrorAction SilentlyContinue -Verbose:$false }     

} else {

    $customModules | Foreach-Object { Import-Module $_.FullName -Force -Verbose:$false }
}

$VerbosePreference = $myVerbosePreference