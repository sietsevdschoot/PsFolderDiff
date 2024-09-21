using module '..\src\DuplicateFileUtils.psm1'

Describe "DuplicateFileUtils" {

    It "Get-Duplicates: Lists all duplicate files" {

        $fileContent1 = [Guid]::NewGuid()
        $fileContent2 = [Guid]::NewGuid()

        10..13 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder1\$_.txt" -Value ($_ % 2 -eq 0 ? $fileContent1 : $fileContent2) -Force }
        20..23 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder2\$_.txt" -Value ($_ % 2 -eq 0 ? $fileContent1 : $fileContent2) -Force }
        30..32 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder3\$_.txt" -Value ($_ % 2 -eq 0 ? $fileContent1 : $fileContent2) -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable
        
        $actual | Should -BeEquivalentTo @(
            @{ Keep="$TestDrive\Folder1\10.txt"; Duplicates=@("$TestDrive\Folder2\20.txt", "$TestDrive\Folder3\30.txt") },
            @{ Keep="$TestDrive\Folder1\11.txt"; Duplicates=@("$TestDrive\Folder2\21.txt", "$TestDrive\Folder3\31.txt") },
            @{ Keep="$TestDrive\Folder1\12.txt"; Duplicates=@("$TestDrive\Folder2\22.txt", "$TestDrive\Folder3\32.txt") },
            @{ Keep="$TestDrive\Folder1\13.txt"; Duplicates=@("$TestDrive\Folder2\23.txt" ) }
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