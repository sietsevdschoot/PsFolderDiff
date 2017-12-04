[CmdletBinding()]
param ()

$modules = dir $PSScriptRoot\*.psm1 -Recurse -Exclude @("Pester.psm1","FileHashLookup.Impl.psm1") | %{ $_.FullName -replace $_.Extension }

foreach ($module in $modules) {

    if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent) 
    {
        Import-Module $module -Force
    }
    else 
    {
        $output = Import-Module $module -Force -Verbose:$true 4>&1

        $output | ?{ $line = $_; ( @("*Importing*", "*Loading*") | ?{ $line -like $_ }) -ne $null } | %{ $_ -replace "VERBOSE: " }
    }
}