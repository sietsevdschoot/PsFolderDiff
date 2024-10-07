using module '..\src\DuplicateFileUtils.psm1'
using module '..\src\FileHashLookup.Impl.psm1'
using module '..\src\BasicFileInfo.psm1'

Describe "DuplicateFileUtils" {

    It "Get-Duplicates: Lists all duplicate files" {

        $fileContents = 1..4 | ForEach-Object { [Guid]::NewGuid() } 

        10..14 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder1\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }
        20..23 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder2\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }
        30..32 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder3\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable

        $simplifiedActual = $actual | Select-Object `
            @{ Name="Keep"; Expression={$_.Keep.FullName} },
            @{ Name="Duplicates"; Expression={ ,@($_.Duplicates | ForEach-Object { $_.FullName }) } }

        $simplifiedActual | Should -BeEquivalentTo @(
            [PsCustomObject]@{ Keep="$TestDrive\Folder1\10.txt"; Duplicates=@("$TestDrive\Folder2\20.txt", "$TestDrive\Folder3\30.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder1\11.txt"; Duplicates=@("$TestDrive\Folder2\21.txt", "$TestDrive\Folder3\31.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder1\12.txt"; Duplicates=@("$TestDrive\Folder2\22.txt", "$TestDrive\Folder3\32.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder1\13.txt"; Duplicates=@("$TestDrive\Folder2\23.txt" ) }
        ) 
    }

    It "Get-Duplicates: Can pass custom sort expressions" {

        $fileContents = 1..4 | ForEach-Object { [Guid]::NewGuid() } 

        10..14 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder1\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }
        20..23 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder2\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }
        30..32 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder3\$_.txt" -Value ($fileContents[($_ % 10)]) -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable -SortBy { param([IO.FileInfo[]] $files) $files | Sort-Object -prop @{ Expression={$_.Directory.FullName}; Descending=$true } } 

        $simplifiedActual = $actual | Select-Object `
            @{ Name="Keep"; Expression={$_.Keep.FullName} },
            @{ Name="Duplicates"; Expression={ ,@($_.Duplicates | ForEach-Object { $_.FullName }) } }

        $simplifiedActual | Should -BeEquivalentTo @(
            [PsCustomObject]@{ Keep="$TestDrive\Folder3\30.txt"; Duplicates=@("$TestDrive\Folder2\20.txt", "$TestDrive\Folder1\10.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder3\31.txt"; Duplicates=@("$TestDrive\Folder2\21.txt", "$TestDrive\Folder1\11.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder3\32.txt"; Duplicates=@("$TestDrive\Folder2\22.txt", "$TestDrive\Folder1\12.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder2\23.txt"; Duplicates=@("$TestDrive\Folder1\13.txt" ) }
        ) 
    }

    BeforeEach {
        
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }

        Set-Location $TestDrive
    }

    AfterEach {
        
        Get-ChildItem $TestDrive -Directory -Recurse | Remove-Item -Force -Recurse
        Get-ChildItem $TestDrive -file -Recurse | Remove-Item -Force
    }    

    BeforeAll {

        & ".\Reload.ps1"

        $originalLocation = Get-Location

        Import-Module $PSScriptRoot\Extensions\PesterExtensions.psm1 -Force
    
        Add-ShouldOperator -Name BeEquivalentTo -Test $function:BeEquivalentTo -SupportsArrayInput
        Add-ShouldOperator -Name ContainEquivalentOf -Test $function:ContainEquivalentOf -SupportsArrayInput
    }

    AfterAll {

        Set-Location $originalLocation

        & ".\Reload.ps1" -unload
    }
}