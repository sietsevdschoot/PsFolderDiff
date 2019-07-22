using module '.\DuplicateFileUtils.psm1'

$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Describe "DuplicateFileUtils" {

    BeforeEach {
    
        & "$here\Reload.ps1"
        
        $originalLocation = Get-Location 
        Set-Location $TestDrive
    }

    AfterEach {
        
        & "$here\Reload.ps1" -unload
        
        Set-Location $originalLocation
    
        Get-ChildItem $TestDrive -Directory -Recurse | Remove-Item -Force -Recurse
        Get-ChildItem $TestDrive -file -Recurse | Remove-Item -Force
    } 
    
    It "Get-FoldersContainingDuplicates: List all folders containing duplicates" {

        $fileContent = [Guid]::NewGuid()

        1..3 | ForEach-Object { New-Item -ItemType File "$TestDrive\RootFolder\Folder1\SubFolder1\Sub\$_.txt" -Value $fileContent -Force }
        4..6 | ForEach-Object { New-Item -ItemType File "$TestDrive\RootFolder\Folder1\SubFolder2\Sub\$_.txt" -Value $fileContent -Force }

        $fileHashTable = GetFileHashTable $TestDrive\RootFolder
        
        $actual = (Get-FoldersContainingDuplicates $fileHashTable) | Select-Object -exp Path | Sort-Object
        
        $expected = @(
            "$TestDrive\RootFolder",
            "$TestDrive\RootFolder\Folder1",
            "$TestDrive\RootFolder\Folder1\SubFolder1",
            "$TestDrive\RootFolder\Folder1\SubFolder1\Sub",
            "$TestDrive\RootFolder\Folder1\SubFolder2",
            "$TestDrive\RootFolder\Folder1\SubFolder2\Sub")

        $actual | Should -Be $expected
    }

    It "Get-DuplicatesByName: Returns a hashtable of duplicates by only looking at filename" {
    
        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\RootFolder\Folder1\SubFolder1\Sub\$_.txt" -Value "$([Guid]::NewGuid().ToString("N"))" -Force }
        3..6 | ForEach-Object { New-Item -ItemType File "$TestDrive\RootFolder\Folder1\SubFolder2\Sub\$_.txt" -Value "$([Guid]::NewGuid().ToString("N"))" -Force }

        $fileHashTable = GetFileHashTable $TestDrive\RootFolder\Folder1
        
        $actual = Get-DuplicatesByName $fileHashTable

        $expected = @{
            
            "3.txt" = @("$TestDrive\RootFolder\Folder1\SubFolder1\Sub\3.txt","$TestDrive\RootFolder\Folder1\SubFolder2\Sub\3.txt");
            "4.txt" = @("$TestDrive\RootFolder\Folder1\SubFolder1\Sub\4.txt","$TestDrive\RootFolder\Folder1\SubFolder2\Sub\4.txt");
        }

        $expectedEntries = $expected.GetEnumerator() | ForEach-Object { $_ }
        $actualEntries = $actual.GetEnumerator() | ForEach-Object { $_ }
        
        ($actualEntries | Select-Object -exp Key) | Should -Be ($expectedEntries | Select-Object -exp Key)
        ($actualEntries | Select-Object -exp Value) | Should -Be ($expectedEntries | Select-Object -exp Value)
    }
}