using module '..\src\DuplicateFileUtils.psm1'

Describe "DuplicateFileUtils" {

    It "Get-Duplicates: List all folders containing duplicates" -Skip {

        $fileContent = [Guid]::NewGuid()

        1..3 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder1\$_.txt" -Value $fileContent -Force }
        4..6 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder2\$_.txt" -Value $fileContent -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable
        
        $expected = @(
            "$TestDrive\RootFolder",
            "$TestDrive\RootFolder\Folder1",
            "$TestDrive\RootFolder\Folder1\SubFolder1",
            "$TestDrive\RootFolder\Folder1\SubFolder1\Sub",
            "$TestDrive\RootFolder\Folder1\SubFolder2",
            "$TestDrive\RootFolder\Folder1\SubFolder2\Sub")

        $actual | Should -Be $expected
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
    }

    AfterAll {

        Set-Location $originalLocation

        & ".\Reload.ps1" -unload
    }
}