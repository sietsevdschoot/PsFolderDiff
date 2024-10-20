

<#
    .SYNOPSIS
    Allows for comparison of folder contents.
    .DESCRIPTION
    Builds a [SyncFileHashLookup] of the contents of a directory. 
    generates a two-way hashs table of the contents of a directory
    .PARAMETER path
    Path to folder to build a file hash table of.
    .EXAMPLE
    First Example
    .EXAMPLE
    Second Example
#>
function Get-FileHashTable {
  [CmdletBinding()]
  param (
      [IO.DirectoryInfo] $path = $null
  )

  $settings = Get-FileHashLookupSettings

  $fileHashLookup = [PsFolderDiff.FileHashLookupLib.Services.SyncFileHashLookup]::Create($settings)

  if ($path) {
    
    $fileHashLookup.Include($path.FullName)
  }

  $fileHashLookup
}

<#
  .SYNOPSIS
  Loads a [SyncFileHashLookup] from a file
  .DESCRIPTION
  Loads a [SyncFileHashLookup] from a file
  .PARAMETER path
  Path to previously saved .json file.
  .EXAMPLE
  First Example
  .EXAMPLE
  Second Example
#>
function Import-FileHashTable {
  [CmdletBinding()]
  param (
      [IO.FileInfo] $file
  )
  $settings = Get-FileHashLookupSettings

  [PsFolderDiff.FileHashLookupLib.Services.SyncFileHashLookup]::Load($file.FullName, $settings)    
}

Function Get-FileHashLookupSettings {

  [Management.Automation.Runspaces.Runspace]::DefaultRunspace = [RunspaceFactory]::CreateRunspace()
  # $settings = [PsFolderDiff.FileHashLookupLib.Configuration.FileHashLookupSettings]::new()
  $settings = [PsFolderDiff.FileHashLookupLib.Configuration.FileHashLookupSettings]::Default

  # $settings.ReportProgress = [System.Progress[PsFolderDiff.FileHashLookupLib.Domain.ProgressEventArgs]]::new({param($progress) Write-Progress @progress })
  $settings.ReportProgress = [System.Progress[PsFolderDiff.FileHashLookupLib.Domain.ProgressEventArgs]]::new({param($progress) Write-Host "hoi" })
  $settings.ReportProgressDelay = [TimeSpan]::Zero

  $settings
}

Function Import-RequiredDependencies {

  $hasLoadedDependencies = try { [PsFolderDiff.FileHashLookupLib.Services.SyncFileHashLookup] > $null; $true } catch { $false }
  
  if (!$hasLoadedDependencies) {

    Import-Module (Join-Path $PSScriptRoot ..\..\..\myscripts\PowerShell\NugetUtils.psm1 -Resolve) -Force -Verbose

    @(
      @{ NugetPackage = "MediatR"; RequiredVersion = "12.4.1" },
      @{ NugetPackage = "MediatR.Contracts"; RequiredVersion = "2.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.Abstractions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.Binder"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.DependencyInjection"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.DependencyInjection.Abstractions"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.FileSystemGlobbing"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Logging"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Abstractions"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Configuration"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Console"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.Options"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.Options.ConfigurationExtensions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Primitives"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Newtonsoft.Json"; RequiredVersion = "13.0.3" },
      @{ NugetPackage = "System.IO.Abstractions"; RequiredVersion = "21.0.29" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions"; RequiredVersion = "21.0.29" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions.TestingHelpers"; RequiredVersion = "21.0.29" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions.Wrappers"; RequiredVersion = "21.0.29" },
      @{ NugetPackage = "Vipentti.IO.Abstractions.FileSystemGlobbing"; RequiredVersion = "1.0.4" }
    ) | ForEach-Object { $nugetArgs = $_; Install-NugetPackage @nugetArgs -Path "$((Join-Path $PSScriptRoot ..\src\lib\))" -LatestVersion }
    
    Import-Assemblies -Path "$((Join-Path $PSScriptRoot ..\src\lib\))" -Verbose
    Import-Assemblies -Path "$((Join-Path $PSScriptRoot ..\tools\FolderDiff\src\FileHashLookupLib\bin\Debug\net8.0\))"
  }
}

Import-RequiredDependencies

Set-Alias GetFileHashTable Get-FileHashTable
Set-Alias Get-HashTable Get-FileHashTable

Set-Alias ImportFileHashTable Import-FileHashTable
Set-Alias Import-HashTable Import-FileHashTable

Export-ModuleMember -Function Get-FileHashTable -Alias @("GetFileHashTable","Get-HashTable")
Export-ModuleMember -Function Import-FileHashTable -Alias @("ImportFileHashTable","Import-HashTable")