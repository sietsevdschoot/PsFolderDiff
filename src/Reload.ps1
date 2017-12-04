[cmdletbinding()]
param (
    [switch] $unload
)

$customModules = dir $PSScriptRoot\*.psm1 -recurse -Exclude @("Pester.psm1")

if ($unload) {

    $customModules | %{ Remove-Module ($_.Name -replace $_.Extension) -Force -ErrorAction SilentlyContinue -Verbose:$false }     

} else {

    $customModules | ?{ @("FileHashLookup.Impl.psm1") -inotcontains $_.Name } |  %{ Import-Module ($_.FullName -replace $_.Extension) -Force -Verbose:$false }
}
