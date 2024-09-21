using module '..\src\FileHashLookup.Impl.psm1'
using module '..\src\BasicFileInfo.psm1'
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

$testResults = [List[PsCustomObject]]@()

if (!$testFile) {

    $testFiles = Get-ChildItem (Join-Path $PSScriptRoot *.Tests.*) 

    foreach ($testFile in $testFiles) {

        $testResults.Add((Invoke-Pester -Script $_.FullName -PassThru))
    }
    
} else {

    $testResults.Add((Invoke-Pester -Script $testFile -PassThru))
}

$testResults | ForEach-Object { $_.Failed | Select-Object ExpandedPath, ErrorRecord } 

Set-Location $location