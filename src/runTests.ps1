[Cmdletbinding()]
param (
    [IO.FileInfo] $testFile
)

$PesterVersion = '4.1.0'

$location = Get-Location

Set-Location $psScriptRoot

if (!(Test-Path ".modules")) {
    New-Item -ItemType Directory .modules -force > $null
}

if (!(Test-Path ".\.modules\Pester\$PesterVersion")) {

    Save-Module -Name Pester -Path '.modules\' -RequiredVersion $PesterVersion
}

# Copy custom assertions
Copy-Item -Path '.\Assertions\*.ps1' -Destination ".\.modules\Pester\$PesterVersion\Functions\Assertions"

# Import local Pester module so we can extend built-in assertions
Import-Module ".\.modules\Pester\$PesterVersion\Pester.psd1" -Force -Verbose:$false

# Run tests
if (!$testFile) {
   
    Get-ChildItem $PSscriptRoot\*.Tests.* | %{ Invoke-Pester -Script $_.FullName }
    
} else {

    Invoke-Pester -Script $testFile
}



Set-Location $location