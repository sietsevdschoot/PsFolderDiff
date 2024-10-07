using namespace System.Collections.Generic

[Cmdletbinding()]
param (
    [IO.FileInfo] $testFile
)

Set-Location $psScriptRoot

if (!(Get-Module Pester)) {
    
    Import-Module Pester -Force
}

[PesterConfiguration]::Default.Debug.ShowFullErrors = $true
[PesterConfiguration]::Default.Debug.WriteDebugMessages = $true
[PesterConfiguration]::Default.Output.Verbosity = "Diagnostic"

# Run tests
$testFiles = Get-ChildItem (Join-Path $PSScriptRoot *.Tests.*) 

if ($testFile) {

    $testFiles =  (,@($testFile))
    
}

$testResults = $testFiles | ForEach-Object { Invoke-Pester -Script $_.FullName -PassThru } 

$testResults | ForEach-Object { $_.Failed | Select-Object ExpandedPath, ErrorRecord } 

Set-Location $location