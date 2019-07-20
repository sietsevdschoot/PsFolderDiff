[CmdletBinding()]
param ()

$modules = Get-ChildItem $PSScriptRoot\*.psm1 -Recurse -Exclude @("Pester.psm1")

foreach ($module in $modules) {

    Remove-Module ($module.FullName -replace $module.Extension) -ErrorAction SilentlyContinue
    
    $output = Import-Module $module -Force -Verbose:($VerbosePreference -eq 'Continue') 4>&1

    $output | Where-Object { $line = $_; ( @("*Importing*", "*Loading*") | Where-Object { $line -like $_ }) -ne $null } | Foreach-Object { $_ -replace "VERBOSE: " }
}