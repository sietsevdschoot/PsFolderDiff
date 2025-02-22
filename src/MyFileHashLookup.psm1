using namespace PsFolderDiff.FileHashLookupLib.Domain
using namespace PsFolderDiff.FileHashLookupLib.Configuration
using namespace PsFolderDiff.FileHashLookupLib.Services
using namespace PsFolderDiff.FileHashLookupLib.Utils

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

  $fileHashLookup = [SyncFileHashLookup]::Create($settings)

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

  [SyncFileHashLookup]::Load($file.FullName, $settings)    
}

Function Get-FileHashLookupSettings {

  $settings = [FileHashLookupSettings]::Default
  $settings.ReportProgress = [PowershellProgressAction[PsFolderDiff.FileHashLookupLib.Domain.ProgressEventArgs]]::new($Host, { 
    param ([PsFolderDiff.FileHashLookupLib.Domain.ProgressEventArgs] $progressArgs) 
    
    $progress = @{}
    $progressArgs.psobject.properties | Where-Object { $_.Value } | ForEach-Object { $progress[$_.Name] = $_.Value  }; 

    Write-Progress @progress
  })

  $settings
}

Function Import-RequiredDependencies {

  $hasLoadedDependencies = try { [SyncFileHashLookup] > $null; $true } catch { $false }
  
  if (!$hasLoadedDependencies) {

    Import-Module (Join-Path $PSScriptRoot ..\..\..\myscripts\PowerShell\NugetUtils.psm1 -Resolve) -Force -Verbose

    @(
      @{ NugetPackage = "MediatR"; RequiredVersion = "12.4.1" },
      @{ NugetPackage = "MediatR.Contracts"; RequiredVersion = "2.0.1" },
      @{ NugetPackage = "Microsoft.ApplicationInsights"; RequiredVersion = "2.21.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.Abstractions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.Binder"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.FileExtensions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Configuration.Json"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.DependencyInjection"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.DependencyInjection.Abstractions"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.FileProviders.Abstractions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.FileProviders.Embedded"; RequiredVersion = "8.0.10" },
      @{ NugetPackage = "Microsoft.Extensions.FileProviders.Physical"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.FileSystemGlobbing"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Logging"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Abstractions"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Configuration"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Logging.Console"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Options"; RequiredVersion = "8.0.2" },
      @{ NugetPackage = "Microsoft.Extensions.Options.ConfigurationExtensions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Extensions.Primitives"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Microsoft.Management.Infrastructure"; RequiredVersion = "3.0.0" },
      @{ NugetPackage = "Microsoft.Management.Infrastructure.Runtime.Unix"; RequiredVersion = "3.0.0" },
      @{ NugetPackage = "Microsoft.Management.Infrastructure.Runtime.Win"; RequiredVersion = "3.0.0" },
      @{ NugetPackage = "Microsoft.PowerShell.CoreCLR.Eventing"; RequiredVersion = "7.4.6" },
      @{ NugetPackage = "Microsoft.PowerShell.Native"; RequiredVersion = "7.4.0" },
      @{ NugetPackage = "Microsoft.Security.Extensions"; RequiredVersion = "1.2.0" },
      @{ NugetPackage = "Microsoft.Win32.Registry.AccessControl"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "Newtonsoft.Json"; RequiredVersion = "13.0.3" },
      @{ NugetPackage = "NLog"; RequiredVersion = "5.3.4" },
      @{ NugetPackage = "NLog.Extensions.Logging"; RequiredVersion = "5.3.14" },
      @{ NugetPackage = "System.CodeDom"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Configuration.ConfigurationManager"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "System.Diagnostics.DiagnosticSource"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "System.Diagnostics.EventLog"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "System.DirectoryServices"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Formats.Asn1"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "System.IO.Abstractions"; RequiredVersion = "21.1.3" },
      @{ NugetPackage = "System.Management"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Management.Automation"; RequiredVersion = "7.4.6" },
      @{ NugetPackage = "System.Security.AccessControl"; RequiredVersion = "6.0.1" },
      @{ NugetPackage = "System.Security.Cryptography.Pkcs"; RequiredVersion = "8.0.1" },
      @{ NugetPackage = "System.Security.Cryptography.ProtectedData"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Security.Permissions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Text.Encoding.CodePages"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Text.Encodings.Web"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Text.Json"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "System.Windows.Extensions"; RequiredVersion = "8.0.0" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions"; RequiredVersion = "21.1.3" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions.TestingHelpers"; RequiredVersion = "21.1.3" },
      @{ NugetPackage = "TestableIO.System.IO.Abstractions.Wrappers"; RequiredVersion = "21.1.3" },
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