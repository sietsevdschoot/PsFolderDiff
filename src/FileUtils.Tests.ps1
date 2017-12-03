using module '.\FileUtils.psm1'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Describe "FileUtils" {

    It "MoveFolder-KeepExisting: Copies folder to targetFolder, removes source folders" {
    
        New-Item -ItemType File "$Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc" -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\administration\Invoices -Force
    
        [IO.DirectoryInfo]"$Testdrive\backup\MyDocuments2015\Administration\Invoices" | Move-FolderKeepExisting $Testdrive\MyDocuments\
        
        "$Testdrive\MyDocuments\InVoices\MyInvoice.doc" | Should -Exist
        "$Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc" | Should -Not -Exist
    }
    
    It "CopyFolder-KeepExisting: DirectlyIntoTargetFolder skips creation of parent folder" {
    
        New-Item -ItemType File "$Testdrive\backup\MyDocuments2015\Administration\Invoices\SubFolder\MyInvoice.doc" -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\administration\Invoices -Force
    
        [IO.DirectoryInfo]"$Testdrive\backup\MyDocuments2015\Administration\Invoices" | Copy-FolderKeepExisting $Testdrive\MyDocuments\ -directlyIntoTargetFolder
        
        "$Testdrive\MyDocuments\SubFolder\MyInvoice.doc" | Should -Exist
    }
    
    It "CopyFolder-KeepExisting: Copies folder to targetFolder" {
    
        New-Item -ItemType File "$Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc" -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\administration\Invoices -Force
    
        [IO.DirectoryInfo]"$Testdrive\backup\MyDocuments2015\Administration\Invoices" | Copy-FolderKeepExisting $Testdrive\MyDocuments\
        
        "$Testdrive\MyDocuments\InVoices\MyInvoice.doc" | Should -Exist
    }

    It "Copy-KeepExisting: Can keep existing file" {

        $file = New-Item -ItemType File $Testdrive\MyFolder\test.txt -Force
    
        1..3 | %{ $file | Copy-KeepExisting $Testdrive\MyFolder } 
        
        $filenames = dir "$Testdrive\MyFolder\test - Copy*.txt" | %{ $_.Name -replace $_.Extension } | Sort
        $filenames | Should Be @("test - Copy", "test - Copy (2)", "test - Copy (3)")
    }
    
    # Path matching examples:
 
    # Given:  $Testdrive\MyDocuments\administration\Invoices
    # File:   $Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc
    # Dest:   $Testdrive\MyDocuments\
    
    # Result: $Testdrive\MyDocuments\Administration\Invoices\MyInvoice.doc


    # Given:  $Testdrive\MyDocuments\administration\Invoices
    # File:   $Testdrive\backup\MyDocuments2015\Administration\Registrations\MyRegistration.doc
    # Dest:   $Testdrive\MyDocuments\
    
    # Result: $Testdrive\MyDocuments\Administration\Registrations\MyRegistration.doc


    # Given:  $Testdrive\MyDocuments\Administration\Invoices
    # File:   $Testdrive\backup\mp3\somesong.mp3
    # Dest:   $Testdrive\MyDocuments\
    
    # Result: $Testdrive\MyDocuments\backup\mp3\somesong.mp3

    It "Copy-KeepExisting: Skips match to common ancestor when skip flag is passed" {
    
        $file = [IO.FileInfo]"$Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc"
        
        New-Item -ItemType File $file -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\administration\Invoices -Force
    
        $file | Copy-KeepExisting -dest $Testdrive\MyDocuments\
        
        "$Testdrive\MyDocuments\Administration\InVoices\MyInvoice.doc" | Should -Exist
    }

    It "Copy-KeepExisting: Tries to match a path with first common ancestor" {
        
        $file = [IO.FileInfo]"$Testdrive\backup\MyDocuments2015\Administration\Invoices\MyInvoice.doc"
        
        New-Item -ItemType File $file -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\administration\Invoices -Force
    
        $file | Copy-KeepExisting -dest $Testdrive\MyDocuments\
        
        "$Testdrive\MyDocuments\Administration\InVoices\MyInvoice.doc" | Should -Exist
    }

    It "Copy-KeepExisting: If first common ancester is found, creates subsequent child folders" {
        
        $file = "$Testdrive\backup\MyDocuments2015\Administration\Registrations\MyRegistration.doc"
        
        New-Item -ItemType File $file -Force
        New-Item -ItemType Directory $Testdrive\MyDocuments\Administration\InVoices -Force
    
        $file | Copy-KeepExisting -dest $Testdrive\MyDocuments\
        
        "$Testdrive\MyDocuments\Administration\Registrations\MyRegistration.doc" | Should -Exist
    }

    It "Copy-KeepExisting: If no common ancester is found, creates as child of destination path" {
        
        # If a common ancestor cannot be found, this logic has to strip the directoryRoot of the file and join it with the destinationPath.
        # To work on the Directory.Root property, we have to map $TestDrive to an actual temporary drive, which we'll remove after we're done. 

        $driveLetter = 'W'
        
        if ((Get-PSDrive -Name $driveLetter -ErrorAction SilentlyContinue) -ne $null) {
            
            Remove-PSDrive -Name $driveLetter
        }
                
        $testDrivePath = Join-Path \\localhost ($Testdrive -replace ':', '$' )  
        New-PSDrive -Name $driveLetter -PSProvider FileSystem -Root $testDrivePath -Scope Global
        
        $file = "$($driveLetter):\backup\mp3\somesong.mp3"

        New-Item -ItemType File $file -Force
        New-Item -ItemType Directory "$($driveLetter):\MyDocuments\Administration\Invoices" -Force

        $file | Copy-KeepExisting -dest "$($driveLetter):\MyDocuments\"

        "$Testdrive\MyDocuments\backup\mp3\somesong.mp3" | Should -Exist

        Remove-PSDrive -Name $driveLetter
    }

    It "Remove-EmptyFolders: deletes all folder which don't contain any files." {
    
        New-Item -ItemType Directory $TestDrive\Temp\Folder1\Sub1 -Force 
        New-Item -ItemType Directory $TestDrive\Temp\Folder1\Sub2 -Force 
        New-Item -ItemType Directory $TestDrive\Temp\Folder1\Sub3 -Force 
        New-Item -ItemType Directory $TestDrive\Temp\Folder2\Sub1 -Force 
        
        New-Item -ItemType File $TestDrive\Temp\Folder2\Sub2\document.txt -Force 

        Remove-EmptyFolders $TestDrive\Temp\

        (dir $TestDrive\Temp\ -Directory -Recurse).Length | Should -Be 2    }

    BeforeEach {
    
        & "$here\Reload.ps1"
    
        dir $TestDrive -Directory | del -Force -Recurse
        dir $TestDrive -File | del -Force

        $originalLocation = Get-Location 
        Set-Location $TestDrive
    }
   
    AfterEach {
        
        & "$here\Reload.ps1" -unload
        
        Set-Location $originalLocation
    
        dir $TestDrive -Directory | del -Force -Recurse
        dir $TestDrive -File | del -Force
    }

}
