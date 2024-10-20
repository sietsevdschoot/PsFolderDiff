[CmdletBinding()]
param(
  [switch] $RunTests,
  [switch] $Verify
)

Import-Module (Join-Path $PSScriptRoot ..\..\..\myscripts\PowerShell\NugetUtils.psm1 -Resolve) -Force -Verbose

if ($RunTests.IsPresent) {

  Get-ChildItem (Join-Path $PSScriptRoot "..\tools\FolderDiff\") -Include "obj", "bin" -Recurse | ForEach-Object { Write-Host $_.FullName; Remove-Item $_ -Force -Recurse }; 
  dotnet test (Join-Path $PSScriptRoot "..\tools\FolderDiff\tests\UnitTests\UnitTests.csproj")
}

if ($LASTEXITCODE -eq 0) {

  $csProjectArgs = @{
    CsProjectFile = (Join-Path $PSScriptRoot "..\tools\FolderDiff\src\FileHashLookupLib\FileHashLookupLib.csproj");
    PackagesToIgnore = @("SecurityCodeScan.VS2019", "StyleCop.Analyzers");
  }

  Build-NugetImportStatementsForCsProject @csProjectArgs
}

if ($Verify) {

  @(
    @{ NugetPackage = "Microsoft.Extensions.DependencyInjection"; RequiredVersion = "8.0.1" },
    @{ NugetPackage = "Microsoft.Extensions.DependencyInjection.Abstractions"; RequiredVersion = "8.0.2" },
    @{ NugetPackage = "Microsoft.Extensions.Logging"; RequiredVersion = "8.0.1" },
    @{ NugetPackage = "Microsoft.Extensions.Logging.Abstractions"; RequiredVersion = "8.0.2" },
    @{ NugetPackage = "Microsoft.Extensions.Logging.Console"; RequiredVersion = "8.0.1" },
    @{ NugetPackage = "Newtonsoft.Json"; RequiredVersion = "13.0.3" },
    @{ NugetPackage = "System.IO.Abstractions"; RequiredVersion = "21.0.29" },
    @{ NugetPackage = "TestableIO.System.IO.Abstractions.TestingHelpers"; RequiredVersion = "21.0.29" },
    @{ NugetPackage = "Vipentti.IO.Abstractions.FileSystemGlobbing"; RequiredVersion = "1.0.4" },
    @{ NugetPackage = "MediatR"; RequiredVersion = "12.4.1" }
  ) | ForEach-Object { $nugetArgs = $_; Install-NugetPackage @nugetArgs -Path "$((Join-Path $PSScriptRoot ..\src\lib\))" -LatestVersion }
  
  Import-Assemblies -Path "$((Join-Path $PSScriptRoot ..\src\lib\))" -Verbose
  Import-Assemblies -Path "$((Join-Path $PSScriptRoot ..\tools\FolderDiff\src\FileHashLookupLib\bin\Debug\net8.0\))" -Verbose

  $fileHashLookup = [SyncFileHashLookup]::Create()

  $fileHashLookup.Include((Join-Path $PSScriptRoot ..\src\lib\))
}