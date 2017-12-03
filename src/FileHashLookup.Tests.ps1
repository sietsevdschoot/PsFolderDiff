﻿using module '.\FileHashLookup.psm1'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Describe "FileHashLookup" {


    It "Creates 2-way HashTable" {
        
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $actual.File | Should -Not -BeNullOrEmpty
        $actual.Hash | Should -Not -BeNullOrEmpty
    }
    	
	It "Creates a lookup of all files" {
		
		$actual = GetFileHashTable "$TestDrive\MyFolder"
				
        $actual.File.Count | Should -Be 3
        $actual.Hash.Count | Should -Be 3
	}
	
	It "Creates a hash and file lookup" {
    
		$myFile = gi "$TestDrive\MyFolder\1.txt"
		$myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
				
		$actual = GetFileHashTable "$TestDrive\MyFolder"
    
		$actual.File.($myFile.FullName) | Should -Be $myHash
		$actual.Hash.($myHash) | Should -Be $myFile.FullName
	}
    
    It "Exposes the paths which were used" {
        
        New-Item -ItemType Directory "$TestDrive\MyFolder2" -Force 
        New-Item -ItemType Directory "$TestDrive\MyFolder3" -Force 
    
        $actual = GetFileHashTable "$TestDrive\MyFolder2"
    
        $actual.AddFolder("$TestDrive\MyFolder3")
    
        $actual.Paths | Should -Be @("$TestDrive\MyFolder2", "$TestDrive\MyFolder3")
    } 
    
    It "Uses the folder path to generate the filename" {
    
        GetFileHashTable "$TestDrive\MyFolder"
        
        $expectedFileName = ((gi "$TestDrive\MyFolder").FullName -replace (([IO.Path]::GetInvalidFileNameChars() | %{ [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  
    
        $actual = (dir *.xml | Select -First 1).Name
        
        $actual | Should -Be $expectedFileName
    }
    
	It "Creates a file containing the HashTable" { 
    
		$fileHashLookup = GetFileHashTable "$TestDrive\MyFolder"

        $filename =  ((gi "$TestDrive\MyFolder").FullName -replace (([IO.Path]::GetInvalidFileNameChars() | %{ [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  

        $actual = Import-Clixml "$($Testdrive)\$filename"
		
		$actual.File | Should -Not -BeNullOrEmpty	
		$actual.Hash | Should -Not -BeNullOrEmpty
	}
	
    It "Creates an arrayList for files which share the same hash" {
	
	    1..4 | %{ New-Item -ItemType File Testdrive:\MyFolder\IdenticalHash$_.txt -Value "Identical Hash" -Force }
    
	    $myHash = (Get-FileHash -LiteralPath (gi "$TestDrive\MyFolder\IdenticalHash1.txt").FullName -Algorithm MD5).Hash
				
	    $actual = GetFileHashTable "$TestDrive\MyFolder"
			
	    (,$actual.Hash.($myHash)) | Should -BeOfType [Collections.ArrayList]
	    $actual.Hash.($myHash).Count | Should -Be 4
		
	    $actual.File.Count | Should -Be 7
    }
    
    It "will remove entry and ArrayList if no items left" {
    
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $myFile = gi "$TestDrive\MyFolder\1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
    
        $actual.Remove($myFile)
    
        $actual.File.ContainsKey($myFile.FullName) | Should -Be $false
        $actual.Hash.ContainsKey($myHash) | Should -Be $false
    }
    
    It "will remove entry from arrayList if file with same hash exists" {
    
		1..4 | %{ New-Item -ItemType File Testdrive:\MyFolder\IdenticalHash$_.txt -Value "Identical Hash" -Force }
    
		$myFile = gi "$TestDrive\MyFolder\IdenticalHash1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
		
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $actual.Remove($myFile)
    
        $actual.File.ContainsKey($myFile.FullName) | Should -Be $false
        $actual.Hash.($myHash).Count | Should -Be 3 
    }

    It "won't allow save without filename" {
    
        New-Item -ItemType Directory "$TestDrive\MyFolder2" -Force 
    
        $actual = (GetFileHashTable)
    
        $actual.AddFolder("$TestDrive\MyFolder2")
    
        { $actual.Save() } | Should -Throw
    }

    It "When Save is called without filename, uses last known filename" {
    
        $myHash = (GetFileHashTable)

        $myHash.Save("$TestDrive\MyFolder\test1.xml")

        { $myHash.Save() } | Should -Not -Throw 
    }

    It "can load new instance from filename" {
    
        $myHash = GetFileHashTable "$TestDrive\MyFolder"

        $myHash.Save("MyFolder.xml")

        $actual = ([FileHashLookup]::Load("MyFolder.xml"))

        $actual | Should -Not -Be $null
        
        $actual.GetType().Name | Should -Be "FileHashLookup"
    } 

    It "can refresh itself. By adding new files and removing no longer existing files." {
        
        10..11 | %{ New-Item -ItemType File Testdrive:\MyFolder2\$_.txt -Value "My Test Value" -Force  }    
        
        $actual = GetFileHashTable "$TestDrive\MyFolder"
        $actual.AddFolder("$TestDrive\MyFolder2") 
            
        del Testdrive:\MyFolder\1.txt 
        New-Item -ItemType File Testdrive:\MyFolder\4.txt -Force
    
        $actual.Refresh()
    
        $actual.GetFiles() | %{ [int]($_.Name -replace $_.Extension) } | Should -Be @(2,3,4,10,11)
    }

    It "Refresh updates the hash of changed files" {
        
        $actual = GetFileHashTable "$TestDrive\MyFolder"

        $file = gi "$TestDrive\MyFolder\2.txt"

        Set-Content $file -Value "Changed file"

        $file.LastWriteTime = (Get-Date).AddMinutes(15)

        $actual.Refresh()
        
        $actual.File.($file.FullName) | Should -Be (Get-FileHash -LiteralPath $file -Algorithm MD5).Hash
    }

    It "Refresh updated the LastUpdated date" {
    
        $actual = GetFileHashTable

        $lastUpdated = $actual.LastUpdated

        Start-Sleep -Milliseconds 1

        $actual.Refresh()

        $actual.LastUpdated | Should -not -be $lastUpdated
    }

    It "Sets the LastUpdated time when FileHashTable on instantiation" {
    
         (GetFileHashTable "$TestDrive\MyFolder").LastUpdated | Should -Not -Be $null
         (GetFileHashTable).LastUpdated | Should -Not -Be $null
    }

    It "returns unique items from other object it is compared to" {
        
        $myHash = GetFileHashTable "$TestDrive\MyFolder"
        
        4..8 | %{ New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder"

        $actual = $myHash.GetDiffInOther($newHash)

        $actual.GetFiles() | %{ [int]($_.Name -replace $_.Extension) } | Should -Be @(4,5,6,7,8)
    }
   
    It "returns matching items from other object it is compared to" {
        
        $myHash = GetFileHashTable "$TestDrive\MyFolder"
        
        4..8 | %{ New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder"

        $actual = $myHash.GetMatchesInOther($newHash)

        $actual.GetFiles() | %{ [int]($_.Name -replace $_.Extension) } | Should -Be @(1, 2, 3)
    }

    It "will look at the file hash to determine file equality or difference" {
        
        1..3 | %{ New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value" -Force  }    

        $myHash = GetFileHashTable "$TestDrive\MyFolder"

        4..5 | %{ New-Item -ItemType File Testdrive:\MyFolder2\$_.txt -Value "My Test Value" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder2"

        $actual = $myHash.GetMatchesInOther($newHash)

        $actual.GetFiles() | %{ [int]($_.Name -replace $_.Extension) } | Should -Be @(4, 5)
    }

    It "will not contain duplicates files" {
    
        $myHash = [FileHashLookup]::New()

        $file1 = gi "$TestDrive\MyFolder\1.txt"
        $file1Hash = (Get-FileHash -LiteralPath $file1 -Algorithm MD5).Hash

        $myHash.Add($file1.FullName)
        $myHash.Add($file1.FullName)

        $myHash.Hash.($file1Hash).Count | Should -Be 1
    }
    
    It "can add other FileHashLookup" {
    
        1..3 | %{ New-Item -ItemType File Testdrive:\Folder1\$_.txt -Force  }    
        4..6 | %{ New-Item -ItemType File Testdrive:\Folder2\$_.txt -Force  }    
    
        $folder1Lookup = GetFileHashTable "$TestDrive\Folder1"
        $folder2Lookup = GetFileHashTable "$TestDrive\Folder2"
    
        $folder1Lookup.AddFileHashTable($folder2Lookup)
    
        $folder1Lookup.GetFiles() | %{ [int]($_.Name -replace $_.Extension) } | Should -Be @(1,2,3,4,5,6)
    }
   
    It "GetFiles: returns all files" {
    
        1..3 | %{ New-Item -ItemType File Testdrive:\Folder1\$_.txt -Force }    

        $expected = dir "$TestDrive\Folder1\*.txt" | Select -exp FullName
        
        $actual = (GetFileHashTable "$TestDrive\Folder1").GetFiles() | Select -exp FullName

        $actual | Should -Be $expected
    }

    It "GetFilesByHash: returns all files matching the hash" {
    
        1..3 | %{ New-Item -ItemType File Testdrive:\Folder1\Apples_$_.txt -Value "Apples" -Force }    
        1..2 | %{ New-Item -ItemType File Testdrive:\Folder1\Oranges_$_.txt -Value "Oranges" -Force }    
    
        $fileHashTable = GetFileHashTable "$TestDrive\Folder1"
        
        $actual = $fileHashTable.GetFilesByHash("$Testdrive\Folder1\Oranges_1.txt")

        $actual.Count | Should -Be 2
    }

    It "Contains: returns if fileHashTable contains file" {
    
        New-Item -ItemType File $Testdrive\Folder1\1.txt -Force

        $actual = GetFileHashTable "$TestDrive\Folder1"

        $actual.Contains("$Testdrive\Folder1\1.txt") | Should -Be $true
        $actual.Contains("$Testdrive\Folder1\2.txt") | Should -Be $false
    }

    It "Can exclude folders" {
    
        $actual = GetFileHashTable
        
        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\2.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\Sub\3.txt" -Force

        $actual.ExludeFolder("$TestDrive\MyFolder1\SubFolder\")

        $actual.AddFolder("$TestDrive\MyFolder1\")

        $actual.GetFiles() | Select -exp FullName | Should -Be @("$TestDrive\MyFolder1\1.txt")
    }

    It "Will remove already added files which match excluded folders" { 

        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\2.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\Sub\3.txt" -Force
    
        $actual = GetFileHashTable $TestDrive\MyFolder1

        $actual.ExludeFolder("$TestDrive\MyFolder1\SubFolder\")

        $actual.GetFiles() | Select -exp FullName | Should -Be @("$TestDrive\MyFolder1\1.txt")
    }

    It "Can exclude file patterns" {

        $actual = GetFileHashTable

        $actual.ExcludeFilePattern("*.txt")
        $actual.ExcludeFilePattern("a*")

         New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

         $actual.AddFolder("$TestDrive\MyFolder1")

         $actual.GetFiles() | Select -exp FullName | Should -Be @("$TestDrive\MyFolder1\resume.pdf")
    }

    It "Will remove already added files which match excludeFilePatterns" {

        $actual = GetFileHashTable

        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

        $actual.AddFolder("$TestDrive\MyFolder1")

        $actual.ExcludeFilePattern("*.txt")
        $actual.ExcludeFilePattern("a*")

        $actual.GetFiles() | Select -exp FullName | Should -Be @("$TestDrive\MyFolder1\resume.pdf")
    }

    It "Can add relative folders" {
        
        1..4 | %{ New-Item -ItemType File "$TestDrive\MyFolder\SubFolder\$_.txt" -Force }

        cd $TestDrive\MyFolder

        $actual = GetFileHashTable .\SubFolder

        ($actual.GetFiles()).Count | Should -Be 4
    }

    BeforeEach {
    
        & "$here\Reload.ps1"
        
        $originalLocation = Get-Location 
        Set-Location $TestDrive
    
        1..3 | %{ New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }
    }

    AfterEach {
        
        & "$here\Reload.ps1" -unload
        
        Set-Location $originalLocation
    
        dir $TestDrive -file -Recurse | del -Force
    }
}