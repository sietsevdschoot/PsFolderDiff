using namespace System.Collections.Generic

Describe "FileHashLookup" {

    BeforeAll {

        & ".\Reload.ps1"

        $originalLocation = Get-Location
        $ProgressPreference = "SilentlyContinue"
    }

    AfterAll {

        Set-Location $originalLocation

        & ".\Reload.ps1" -unload
        $ProgressPreference = "Continue"
    }
    
    BeforeEach {
        
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }
        Set-Location $TestDrive
    }

    AfterEach {
        
        Get-ChildItem $TestDrive -Directory -Recurse | Remove-Item -Force -Recurse
        Get-ChildItem $TestDrive -file -Recurse | Remove-Item -Force
    }    

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
    
        $myFile = Get-Item "$TestDrive\MyFolder\1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
                
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $actual.File.($myFile.FullName).Hash | Should -Be $myHash
        $actual.Hash.($myHash).FullName | Should -Be $myFile.FullName
    }
    
    It "Exposes the paths which were used" {
        
        New-Item -ItemType Directory "$TestDrive\MyFolder2" -Force 
        New-Item -ItemType Directory "$TestDrive\MyFolder3" -Force 
    
        $actual = GetFileHashTable "$TestDrive\MyFolder2"
    
        $actual.AddFolder("$TestDrive\MyFolder3")
    
        @($actual.Paths) | Should -Be @("$TestDrive\MyFolder2", "$TestDrive\MyFolder3")
    } 
    
    It "Uses the folder path to generate the filename" {
    
        New-Item -ItemType File -Path "$TestDrive\My Documents\test.txt" -Force 
        
        GetFileHashTable "$TestDrive\My Documents\"

        $expectedFileName = ((Get-Item "$TestDrive\My Documents\").FullName -replace (([IO.Path]::GetInvalidFileNameChars() + ' ' | ForEach-Object { [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  
    
        $actual = (Get-ChildItem *.xml | Select-Object -First 1).Name
        
        $actual | Should -Be $expectedFileName
    }
    
    It "Creates a file containing the HashTable" { 
    
        GetFileHashTable "$TestDrive\MyFolder"

        $filename =  ((Get-Item "$TestDrive\MyFolder").FullName -replace (([IO.Path]::GetInvalidFileNameChars() | ForEach-Object { [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  

        $actual = Import-Clixml "$($Testdrive)\$filename"
        
        $actual.File | Should -Not -BeNullOrEmpty	
        $actual.Hash | Should -Not -BeNullOrEmpty
    }
    
    It "Creates a List for files which share the same hash" {
            
        1..4 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\IdenticalHash$_.txt -Value "Identical Hash" -Force }
    
        $myHash = (Get-FileHash -LiteralPath (gi "$TestDrive\MyFolder\IdenticalHash1.txt").FullName -Algorithm MD5).Hash
                
        $actual = GetFileHashTable "$TestDrive\MyFolder"

        $actual.Hash[$myHash] -is [System.Collections.Generic.List[BasicFileInfo]] | Should -BeTrue
        $actual.Hash[$myHash].Count | Should -Be 4
        
        $actual.File.Count | Should -Be 7
    }
    
    It "will remove entry and List if no items left" {
    
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $myFile = Get-Item "$TestDrive\MyFolder\1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
    
        $actual.Remove($myFile)
    
        $actual.File.ContainsKey($myFile.FullName) | Should -Be $false
        $actual.Hash.ContainsKey($myHash) | Should -Be $false
    }
    
    It "will remove entry from List if file with same hash exists" {
    
        1..4 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\IdenticalHash$_.txt -Value "Identical Hash" -Force }
    
        $myFile = Get-Item "$TestDrive\MyFolder\IdenticalHash1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
        
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $actual.Remove($myFile)
    
        $actual.File.ContainsKey($myFile.FullName) | Should -Be $false
        $actual.Hash.($myHash).Count | Should -Be 3 
    }

    It "Save without filename will pick tempfile name" {
    
        New-Item -ItemType Directory "$TestDrive\MyFolder2" -Force 
    
        $actual = GetFileHashTable
    
        $actual.AddFolder("$TestDrive\MyFolder2")
    
        { $actual.Save() } | Should -Not -Throw
        ([IO.FileInfo]$actual.SavedAsFile).Directory.FullName | Should -Be (New-TemporaryFile).Directory.FullName
    }

    It "When Save is called without filename, uses last known filename" {
    
        $actual = GetFileHashTable

        $actual.Save("$TestDrive\MyFolder\test1.xml")

        { $actual.Save() } | Should -Not -Throw 
    }

    It "Can save with correct filename when relative path for folder is used" {

        $file = "$TestDrive\MyFolder\Backup\Sub\1.txt"

        New-Item -ItemType File $file -Force
        
        $expectedFileName = ("$TestDrive\MyFolder\Backup" -replace (([IO.Path]::GetInvalidFileNameChars() | ForEach-Object { [Regex]::Escape($_) }) -join "|"), "_") + ".xml"

        Set-Location "$TestDrive\MyFolder"
        
        GetFileHashTable .\Backup 

        "$TestDrive\MyFolder\$expectedFileName" | Should -Exist
    } 

    It "can load new instance from filename" {
    
        4..5 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value" -Force  }

        $myHash = GetFileHashTable "$TestDrive\MyFolder"

        $myHash.Save("$TestDrive\MyFolder.xml")

        $actual = (ImportFileHashTable "$TestDrive\MyFolder.xml")

        $actual | Should  -Not -Be $null
        
        # Note: This line fails every 2nd test run in vscode.
        # $actual | Should -BeOfType [FileHashLookup]
        
        $actual.GetType().Name | Should -Be "FileHashLookup"
    }
    
    It "After load, can retrieve File and hash" {
    
        $myFile = Get-Item "$TestDrive\MyFolder\1.txt"
        $myHash = (Get-FileHash -LiteralPath $myFile.FullName -Algorithm MD5).Hash
                
        $actual = GetFileHashTable "$TestDrive\MyFolder"
    
        $actual.Save()
        $deserialized = [FileHashLookup]::Load($actual.SavedAsFile) 

        $deserialized.File.($myFile.FullName).Hash | Should -Be $myHash
        $deserialized.Hash.($myHash).FullName | Should -Be $myFile.FullName
    }

    It "can refresh itself. By adding new files and removing no longer existing files." {
        
        10..11 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder2\$_.txt -Value "My Test Value" -Force  }    
        
        $actual = GetFileHashTable "$TestDrive\MyFolder"
        $actual.AddFolder("$TestDrive\MyFolder2") 
            
        Remove-Item Testdrive:\MyFolder\1.txt 
        New-Item -ItemType File Testdrive:\MyFolder\4.txt -Force
    
        $actual.Refresh()
    
        $actual.GetFiles() | ForEach-Object { [int]($_.Name -replace $_.Extension) } | Sort-Object | Should -Be @(2,3,4,10,11)
    }

    It "Refresh updates the hash of changed files" {
        
        $path = "$TestDrive\MyFolder"
        
        $actual = GetFileHashTable $path 

        $file = Get-Item "$TestDrive\MyFolder\2.txt"

        Set-Content $file -Value "Changed file"

        $file.LastWriteTime = (Get-Date).AddMinutes(15)

        $files = $actual.InternalGetFiles($path)
        $updatedItems = $actual.InternalGetFilesToAddOrUpdate($path, $files)

        $actual.Refresh()
        
        $updatedItems | Should -HaveCount 2
        $updatedItems | ?{ $_.File.Name -eq "2.txt" } | Select-Object -exp Operation | Should -Be @("Remove", "Add")
        $actual.File.($file.FullName).Hash | Should -Be (Get-FileHash -LiteralPath $file -Algorithm MD5).Hash
    }

    It "Refresh updated the LastUpdated date" {
    
        $actual = GetFileHashTable

        $lastUpdated = $actual.LastUpdated

        Start-Sleep -Milliseconds 3

        $actual.Refresh()

        $actual.LastUpdated | Should -not -be $lastUpdated
    }

    It "Refresh Removes folders and files from paths which do no longer exists." {
    
        $actual = GetFileHashTable

        1..3 | ForEach-Object { New-Item -ItemType Directory Testdrive:\Folders\$_ -Force > $null }
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\Folders\$_\file.txt -Force  > $null }

        Get-ChildItem Testdrive:\Folders -Directory | ForEach-Object { $actual.AddFolder($_) }

        Remove-Item Testdrive:\Folders -Recurse -Force

        $actual.Refresh()

        $actual.Paths | Should -BeNullOrEmpty
        $actual.GetFiles() | Should -BeNullOrEmpty
    }

    It "Sets the LastUpdated time when FileHashTable on instantiation" {
    
         (GetFileHashTable "$TestDrive\MyFolder").LastUpdated | Should -Not -Be $null
         (GetFileHashTable).LastUpdated | Should -Not -Be $null
    }

    It "Returns unique items from other object it is compared to" {
        
        $myHash = GetFileHashTable "$TestDrive\MyFolder"
        
        4..8 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder"

        $actual = $myHash.GetDiffInOther($newHash)

        $actual.GetFiles() | ForEach-Object { [int]($_.Name -replace $_.Extension) } | Should -Be @(4,5,6,7,8)
    }
   
    It "returns matching items from other object it is compared to" {
        
        $myHash = GetFileHashTable "$TestDrive\MyFolder"
        
        4..8 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder"

        $actual = $myHash.GetMatchesInOther($newHash)

        $actual.GetFiles() | ForEach-Object { [int]($_.Name -replace $_.Extension) } | Should -Be @(1, 2, 3)
    }

    It "will look at the file hash to determine file equality or difference" {
        
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value" -Force  }    

        $myHash = GetFileHashTable "$TestDrive\MyFolder"

        4..5 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder2\$_.txt -Value "My Test Value" -Force  }    

        $newHash = GetFileHashTable "$TestDrive\MyFolder2"

        $actual = $myHash.GetMatchesInOther($newHash)

        $actual.GetFiles() | ForEach-Object { [int]($_.Name -replace $_.Extension) } | Should -Be @(4, 5)
    }

    It "will not contain duplicates files" {

        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\SomeFolder\$_.txt -Force }

        $actual = GetFileHashTable 

        Get-ChildItem Testdrive:\SomeFolder\ -File  | ForEach-Object { $actual.Add($_) }

        $file1 = Get-Item "$TestDrive\SomeFolder\1.txt"
        $file1Hash = (Get-FileHash -LiteralPath $file1 -Algorithm MD5).Hash

        $actual.Add($file1.FullName, $file1Hash)

        ($actual.GetFiles() | Select-Object -exp Name) | Should -Be @("1.txt", "2.txt","3.txt")
        $actual.Hash.($file1Hash) | %{ ([IO.FileInfo]$_).Name } | Should -Be @("1.txt", "2.txt","3.txt")
    }
    
    It "can add other FileHashLookup" {
    
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\Folder1\$_.txt -Force  }    
        4..6 | ForEach-Object { New-Item -ItemType File Testdrive:\Folder2\$_.txt -Force  }    
    
        $folder1Lookup = GetFileHashTable "$TestDrive\Folder1"
        $folder2Lookup = GetFileHashTable "$TestDrive\Folder2"
    
        $folder1Lookup.AddFileHashTable($folder2Lookup)
    
        $folder1Lookup.GetFiles() | ForEach-Object { [int]($_.Name -replace $_.Extension) } | Should -Be @(1,2,3,4,5,6)

        $folder1Lookup.Paths | Sort-Object | Should -be @("$TestDrive\Folder1", "$TestDrive\Folder2")
    }
   
    It "GetFiles: returns all files" {
    
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\Folder1\$_.txt -Force }    

        $expected = Get-ChildItem "$TestDrive\Folder1\*.txt" | Select-Object -exp FullName
        
        $actual = (GetFileHashTable "$TestDrive\Folder1").GetFiles() | Select-Object -exp FullName

        $actual | Should -Be $expected
    }

    It "GetFilesByHash: returns all files matching the hash" {
    
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\Folder1\Apples_$_.txt -Value "Apples" -Force }    
        1..2 | ForEach-Object { New-Item -ItemType File Testdrive:\Folder1\Oranges_$_.txt -Value "Oranges" -Force }    
    
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

        $actual.ExcludeFolder("$TestDrive\MyFolder1\SubFolder\")

        $actual.AddFolder("$TestDrive\MyFolder1\")

        $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\1.txt")
    }

    It "Will remove already added files which match excluded folders" { 

        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\2.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\SubFolder\Sub\3.txt" -Force
    
        $actual = GetFileHashTable $TestDrive\MyFolder1

        $actual.ExcludeFolder("$TestDrive\MyFolder1\SubFolder\")

        $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\1.txt")
    }

    It "Can exclude file patterns" {

        $actual = GetFileHashTable

        $actual.ExcludeFilePattern("*.txt")
        $actual.ExcludeFilePattern("a*")

         New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

         $actual.AddFolder("$TestDrive\MyFolder1")

         $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\resume.pdf")
    }

    It "Will remove already added files which match excludeFilePatterns" {

        $actual = GetFileHashTable

        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

        $actual.AddFolder("$TestDrive\MyFolder1")

        $actual.ExcludeFilePattern("*.txt")
        $actual.ExcludeFilePattern("a*")

        $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\resume.pdf")
    }

    It "Can include file patterns" {

        $actual = GetFileHashTable

        $actual.IncludeFilePattern("*.txt")

        New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
        New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

        $actual.AddFolder("$TestDrive\MyFolder1")

        $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\1.txt")
    }

    It "Can exclude file patterns" {

        $actual = GetFileHashTable

        $actual.ExcludeFilePattern("*.txt")
        $actual.ExcludeFilePattern("a*")

         New-Item -ItemType File "$TestDrive\MyFolder1\1.txt" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\administration.doc" -Force
         New-Item -ItemType File "$TestDrive\MyFolder1\resume.pdf" -Force

         $actual.AddFolder("$TestDrive\MyFolder1")

         $actual.GetFiles() | Select-Object -exp FullName | Should -Be @("$TestDrive\MyFolder1\resume.pdf")
    }

    It "Can display contents by using .ToString()" {

        $actual = GetFileHashTable

        $actual.ToString() -like "*Monitored Folders:*" | Should -BeTrue
    } 

    It "Can add relative folders" {
        
        1..4 | ForEach-Object { New-Item -ItemType File "$TestDrive\MyFolder\SubFolder\$_.txt" -Force }

        Set-Location $TestDrive\MyFolder

        $actual = GetFileHashTable .\SubFolder

        $actual.GetFiles() | Should -HaveCount 4
    }
}