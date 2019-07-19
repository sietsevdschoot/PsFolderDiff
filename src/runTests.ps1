[Cmdletbinding()]
param (
#    [IO.FileInfo] $testFile = ".\FileHashLookup.Tests.ps1"
    [IO.FileInfo] $testFile = ".\FileUtils.Tests.ps1"
)

$PesterVersion = '4.1.0'

$location = Get-Location

Set-Location $psScriptRoot

if (!(Test-Path ".modules")) {
    md .modules -force > $null
}

if (!(Test-Path ".\.modules\Pester\$PesterVersion")) {

    Save-Module -Name Pester -Path '.modules\' -RequiredVersion $PesterVersion
}

# Copy custom assertions
Copy-Item -Path '.\Assertions\*.ps1' -Destination ".\.modules\Pester\$PesterVersion\Functions\Assertions"

# Import local Pester module so we can extend built-in assertions
Import-Module ".\.modules\Pester\$PesterVersion\Pester.psd1" -Force -Verbose:$false

# Run tests
Invoke-Pester -Script $testFile

Set-Location $location