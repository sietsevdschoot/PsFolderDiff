[CmdletBinding()]
param ()

$modules = Get-ChildItem $PSScriptRoot\*.psm1 -Recurse -Exclude @("Pester.psm1")
#$modules = Get-ChildItem $PSScriptRoot\*.psm1 -Recurse -Exclude @("Pester.psm1","FileHashLookup.Impl.psm1") | Foreach-Object { $_.FullName -replace $_.Extension }


foreach ($module in $modules) {

    if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) 
    {
        Import-Module $module -Force
    }
    else 
    {
        $output = Import-Module $module -Force -Verbose:$true 4>&1

        $output | Where-Object { $line = $_; ( @("*Importing*", "*Loading*") | Where-Object { $line -like $_ }) -ne $null } | Foreach-Object { $_ -replace "VERBOSE: " }
    }
}