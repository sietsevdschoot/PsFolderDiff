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

Import-Module ".\.modules\Pester\$PesterVersion\Pester.psd1" -Force

Invoke-Pester -Script $testFile

Set-Location $location