using module '..\src\DuplicateFileUtils.psm1'
using module '..\src\FileHashLookup.Impl.psm1'
using module '..\src\BasicFileInfo.psm1'

Describe "DuplicateFileUtils" {

    It "Get-Duplicates: Lists all duplicate files, Sorts by filename" {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable

        $simplifiedActual = $actual | Select-Object `
            @{ Name="Keep"; Expression={$_.Keep.FullName} },
            @{ Name="Duplicates"; Expression={,@($_.Duplicates | ForEach-Object { $_.FullName }) } }

        $simplifiedActual | Should -BeEquivalentTo @(
            [PsCustomObject]@{ Keep="$TestDrive\Folder1\1.txt"; Duplicates=@("$TestDrive\Folder2\2.txt", "$TestDrive\Folder3\3.txt", "$TestDrive\Folder4\4.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder10\10.txt"; Duplicates=@("$TestDrive\Folder11\11.txt", "$TestDrive\Folder12\12.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder20\20.txt"; Duplicates=@("$TestDrive\Folder21\21.txt") }
        ) 
    }

    It "Get-Duplicates: Can pass custom sort expressions - ScriptBlock" {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable -SortScriptBlock { param([IO.FileInfo[]] $files) $files | Sort-Object -prop @{ Expression={$_.FullName}; Descending=$true } } 

        $simplifiedActual = $actual | Select-Object `
            @{ Name="Keep"; Expression={$_.Keep.FullName} },
            @{ Name="Duplicates"; Expression={ ,@($_.Duplicates | ForEach-Object { $_.FullName }) } }

        $simplifiedActual | Should -BeEquivalentTo @(
            [PsCustomObject]@{ Keep="$TestDrive\Folder4\4.txt"; Duplicates=@("$TestDrive\Folder3\3.txt", "$TestDrive\Folder2\2.txt", "$TestDrive\Folder1\1.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder12\12.txt"; Duplicates=@("$TestDrive\Folder11\11.txt", "$TestDrive\Folder10\10.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder21\21.txt"; Duplicates=@("$TestDrive\Folder20\20.txt") }
        ) 
    }

    It "Get-Duplicates: Can pass custom sort expressions - SortExpression" {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable -SortExpression @{ Expression={$_.FullName}; Descending=$true } 

        $simplifiedActual = $actual | Select-Object `
            @{ Name="Keep"; Expression={$_.Keep.FullName} },
            @{ Name="Duplicates"; Expression={ [string[]](,@($_.Duplicates | ForEach-Object { $_.FullName })) } }

        $simplifiedActual | Should -BeEquivalentTo @(
            [PsCustomObject]@{ Keep="$TestDrive\Folder4\4.txt"; Duplicates=@("$TestDrive\Folder3\3.txt", "$TestDrive\Folder2\2.txt", "$TestDrive\Folder1\1.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder12\12.txt"; Duplicates=@("$TestDrive\Folder11\11.txt", "$TestDrive\Folder10\10.txt") },
            [PsCustomObject]@{ Keep="$TestDrive\Folder21\21.txt"; Duplicates=@("$TestDrive\Folder20\20.txt") }
        ) 
    }

    BeforeEach {
        
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