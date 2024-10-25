using namespace System;
using namespace System.Threading;
using namespace Management.Automation.Runspaces;


Add-Type `
    -Language CSharp `
    -ReferencedAssemblies @( `
        (Join-Path $PSScriptRoot "..\tools\FolderDiff\src\FileHashLookupLib\bin\Debug\net8.0\PsFolderDiff.FileHashLookupLib.dll"),
        (Join-Path $PSScriptRoot "..\tools\FolderDiff\tests\UnitTests\bin\Debug\net8.0\PsFolderDiff.FileHashLookupLib.UnitTests.dll"),
        ([Console].Assembly.Location) `
    ) `
    -TypeDefinition (Get-Content (Join-Path $PSScriptRoot ".\code\InlineSampleService2.cs") -Raw);

    try{

        $service = [PsFolderDiff.FileHashLookupLib.UnitTests.InlineSampleService2]::new()

        $myAction = ([Action[string]]{ param([string] $name) Write-Host "Hello $name" })
                
        $service.Execute($null);    
        # $service.Execute({ param([string] $name) Write-Host "Hello $name" });    
        # $service.Execute($myAction);    
    }
    catch {

        $_ | Out-string | Write-Host
    }
