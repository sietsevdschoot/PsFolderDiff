[cmdletbinding()]
param (
    [switch] $unload
)

$customModules = Get-ChildItem $PSScriptRoot\*.psm1 -recurse -Exclude @("Pester.psm1", "FileHashLookup.Impl.psm1")

$myVerbosePreference = $VerbosePreference

$VerbosePreference = "Silent"

if ($unload) {

    $customModules | Foreach-Object { Remove-Module ($_.Name -replace $_.Extension) -Force -ErrorAction SilentlyContinue -Verbose:$false }     

} else {

    $customModules | Foreach-Object { Import-Module ($_.FullName -replace $_.Extension) -Force -Verbose:$false }
}

$VerbosePreference = $myVerbosePreference