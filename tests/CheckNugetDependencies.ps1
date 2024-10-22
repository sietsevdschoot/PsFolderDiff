[CmdletBinding()]
param(
  [switch] $RunTests,
  [switch] $GenerateNugetStatements,
  [switch] $Verify
)

Import-Module (Join-Path $PSScriptRoot ..\..\..\myscripts\PowerShell\NugetUtils.psm1 -Resolve) -Force -Verbose

if ($RunTests.IsPresent) {

  Get-ChildItem (Join-Path $PSScriptRoot "..\tools\FolderDiff\") -Include "obj", "bin" -Recurse | ForEach-Object { Write-Host $_.FullName; Remove-Item $_ -Force -Recurse }; 
  Get-Item (Join-Path $PSScriptRoot "..\src\lib\") | Remove-Item -Recurse -Force
  dotnet test (Join-Path $PSScriptRoot "..\tools\FolderDiff\tests\UnitTests\UnitTests.csproj")
}

if ($GenerateNugetStatements.IsPresent) {

  if (!$LASTEXITCODE -or $LASTEXITCODE -eq 0) {

    $csProjectArgs = @{
      CsProjectFile = (Join-Path $PSScriptRoot "..\tools\FolderDiff\src\FileHashLookupLib\FileHashLookupLib.csproj");
      PackagesToIgnore = @("SecurityCodeScan.VS2019", "StyleCop.Analyzers");
    }
  
    Find-RequiredNugetPackagesForProject @csProjectArgs
  }
}

if ($Verify) {

  Import-Module $PSScriptRoot\..\src\MyFileHashLookup.psm1 -force
  $fileHashLookup = Get-FileHashTable

  $fileHashLookup.Include((Join-Path $PSScriptRoot ..\src\lib\))
}