[cmdletbinding()]
param (
    [switch] $unload
)

$customModules = dir $PSScriptRoot\*.psm1 -recurse -Exclude @("Pester.psm1","FileHashLookup.Impl.psm1")

if ($unload) {

    $customModules | %{ Remove-Module ($_.Name -replace $_.Extension) -Force -ErrorAction SilentlyContinue -Verbose:$false }     

} else {

    $customModules | %{ Import-Module ($_.FullName -replace $_.Extension) -Force -Verbose:$false }
}
