using module '.\FileHashLookup.Impl.psm1'

[Cmdletbinding()]
param (
    [IO.FileInfo] $testFile
)

$location = Get-Location

Set-Location $psScriptRoot

if (!(Get-Module Pester)) {

    Import-Module Pester -Force
}

[PesterConfiguration]::Default.Debug.ShowFullErrors = $true
[PesterConfiguration]::Default.Debug.WriteDebugMessages = $true
[PesterConfiguration]::Default.Output.Verbosity = "normal"


# Run tests
if (!$testFile) {
   
    Get-ChildItem *.Tests.* | ForEach-Object { Invoke-Pester -Script $_.FullName }
    
} else {

    Invoke-Pester -Script $testFile
}

Set-Location $location