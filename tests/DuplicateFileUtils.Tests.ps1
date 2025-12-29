using module '..\src\FileUtils.psm1'
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

        $actual | Should -BeJsonEquivalentTo @(
            [DuplicateFileEntry]::new((1..4 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" })),
            [DuplicateFileEntry]::new((10..12 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" })),
            [DuplicateFileEntry]::new((20..21 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" }))
        )
    }

    It "Get-Duplicates: Can pass custom sort expressions - ScriptBlock" {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable -SortScriptBlock { param([IO.FileInfo[]] $files) $files | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true } }

        $actual | Should -BeJsonEquivalentTo @(
            [DuplicateFileEntry]::new((1..4 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true })),
            [DuplicateFileEntry]::new((10..12 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true })),
            [DuplicateFileEntry]::new((20..21 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true }))
        )
    }

    It "Get-Duplicates: Can pass custom sort expressions - SortExpression" {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable -SortExpression @{ Expression = { $_.FullName }; Descending = $true }

        $actual | Should -BeJsonEquivalentTo @(
            [DuplicateFileEntry]::new((1..4 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true })),
            [DuplicateFileEntry]::new((10..12 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true })),
            [DuplicateFileEntry]::new((20..21 | ForEach-Object { [IO.FileInfo]"$TestDrive\Folder$_\$_.txt" } | Sort-Object -prop @{ Expression = { $_.FullName }; Descending = $true }))
        )
    }

    It "Copy-Duplicates: Copies all duplicate files, keeping folder structure" -skip {

        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File A" -Force }
        10..12 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File B" -Force }
        20..21 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File C" -Force }
        30..30 | ForEach-Object { New-Item -ItemType File "$TestDrive\Folder$_\$_.txt" -Value "File D" -Force }

        $fileHashTable = GetFileHashTable $TestDrive

        $actual = Get-Duplicates $fileHashTable

        $actual | Copy-Duplicates -Destination "$TestDrive\Duplicates"

        Get-ChildItem "$TestDrive\Duplicates" -Recurse | Select-Object -exp FullName | Should -BeEquivalentTo @(
            "$TestDrive\Duplicates\Folder2\2.txt",
            "$TestDrive\Duplicates\Folder3\3.txt",
            "$TestDrive\Duplicates\Folder4\4.txt",
            "$TestDrive\Duplicates\Folder11\11.txt",
            "$TestDrive\Duplicates\Folder12\12.txt",
            "$TestDrive\Duplicates\Folder21\21.txt"
        )

        # Fix Copy-Duplicates matching Directory structure detection.
    }

    BeforeEach {

        Set-Location $TestDrive
    }

    AfterEach {

        if ($TestDrive) {

            Get-ChildItem $TestDrive -Directory -Recurse | Remove-Item -Force -Recurse
            Get-ChildItem $TestDrive -file -Recurse | Remove-Item -Force
        }

    }

    BeforeAll {

        & ".\Reload.ps1"

        $originalLocation = Get-Location

        Import-Module $PSScriptRoot\Extensions\PesterExtensions.psm1 -Force

        Add-ShouldOperator -Name BeEquivalentTo -Test $function:BeEquivalentTo -SupportsArrayInput
        Add-ShouldOperator -Name BeJsonEquivalentTo -Test $function:BeJsonEquivalentTo -SupportsArrayInput
        Add-ShouldOperator -Name ContainEquivalentOf -Test $function:ContainEquivalentOf -SupportsArrayInput
    }

    AfterAll {

        Set-Location $originalLocation

        & ".\Reload.ps1" -unload
    }
}
