Class FileHashLookup 
{
    FileHashLookup() 
    {
        $this.File = @{}
        $this.Hash = @{}
        $this.Paths = [Collections.ArrayList]@()
        $this.ExcludedFilePatterns = [Collections.ArrayList]@()
        $this.ExcludedFolders = [Collections.ArrayList]@()

        $this.LastUpdated = Get-Date
    }

    FileHashLookup([IO.DirectoryInfo] $path)
    {
        $this.File = @{}
        $this.Hash = @{}
        $this.Paths = [Collections.ArrayList]@($path.FullName)
        $this.ExcludedFilePatterns = [Collections.ArrayList]@()
        $this.ExcludedFolders = [Collections.ArrayList]@()

        $this.AddFolder($path)
        $this.LastUpdated = Get-Date

        $fileName = ($path.FullName -replace (([IO.Path]::GetInvalidFileNameChars() | %{ [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  
        
        $this.Save((Join-Path (Get-Location) $fileName))
    }

    hidden [HashTable] $File
    hidden [HashTable] $Hash
    hidden [Collections.ArrayList] $Paths
    hidden [Collections.ArrayList] $ExcludedFilePatterns
    hidden [Collections.ArrayList] $ExcludedFolders
    hidden [DateTime] $LastUpdated
    hidden [string] $SavedAsFile

    [IO.FileInfo[]] GetFiles() {
        
        return $this.File.Keys | Sort | %{ [IO.FileInfo] $_ }
    }

    [IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file) {
        
        if ($this.Contains($file))
        {
            $fileHash = $this.File.($file.FullName)
        } 
        else 
        {
            $fileHash = (Get-FileHash -LiteralPath $file -Algorithm MD5).Hash
        }
        
        return $this.Hash.($fileHash) | Sort | %{ [IO.FileInfo] $_ }
    }

    [bool] Contains([IO.FileInfo] $file) {
        
        return $this.File.ContainsKey($file.FullName)
    }

    AddFolder([IO.DirectoryInfo] $path) {
        
        if (!$path.Exists) {
        
            $path = [IO.DirectoryInfo](Join-Path (Get-Location) $path.Name)
        }
        
        if (!($this.Paths -contains $path.FullName)) {
            
            $this.Paths.Add($path.FullName) > $null
        }
        
        Write-Progress -Activity "Adding or updating files" -Status "Collecting files..."

        $files = dir $path.FullName -File -Recurse -Force -Exclude $this.ExcludedFilePatterns -ErrorAction SilentlyContinue

        if ($this.ExcludedFolders) {
        
            $files = $files | ?{ $file = $_; ($this.ExcludedFolders | ?{ $file.FullName.StartsWith($_) }) -eq $null }
        }
    
        Write-Progress -Activity "Adding or updating files" -Status "Detecting modified files..."

        $files | ?{ $_.LastWriteTime -gt $this.LastUpdated } | %{ $this.Remove($_) }

        Write-Progress -Activity "Adding or updating files" -Status "Analyzing differences..."
         
        $newlyAddedFiles = $files | ?{ !$this.Contains($_.FullName) } | %{ @{ Operation='Add'; File=$_ } }
        $deletedFiles = $this.GetFiles() | ?{ $_ -ne $null -and !$_.Exists } | %{ @{ Operation='Remove'; File=$_  } }
        
        $itemsToUpdate = @($newlyAddedFiles) + @($deletedFiles)

        $sw = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $itemsToUpdate.Count; $i++ ) {
        
            $currentFile = $itemsToUpdate[$i].('File')
            $currentOperation = $itemsToUpdate[$i].('Operation')

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                $activity = if ($currentOperation -eq "Add") { "Calculating Hash..." } else { "Removing from FileHashTable..." }		        
                Write-Progress -Activity $activity -Status "($i of $($itemsToUpdate.Count)) $($currentFile.FullName)" -PercentComple ($i / $itemsToUpdate.Count * 100)
                $sw.Restart()
            }

            if ($currentOperation -eq "Add") 
            {
                $this.Add($currentFile.FullName)    
            } 
            elseif ($currentOperation -eq "Remove") 
            {
                $this.Remove($currentFile.FullName)
            }
        }
    }
    
    Refresh() {
        
       $this.Paths | %{ $this.AddFolder(($_)) }

       $this.LastUpdated = Get-Date
    }

    Save([IO.FileInfo] $filename) {
    
        if ($filename) {
        
            $this.SavedAsFile = if ($filename.Exists) { $filename.FullName } else { (Join-Path (Get-Location) $filename.Name) }
        }
        
        $this.Save()
    }
    
    Save() {
    
        if (!$this.SavedAsFile) {
            Throw "Missing filename for FileHashLookup"
        } 
    
        New-Item -ItemType File $this.SavedAsFile -Force
        Export-Clixml -Path $this.SavedAsFile -InputObject $this        
    }
    
    static [FileHashLookup] Load([IO.FileInfo] $fileToLoad) {
    
        if (!$fileToLoad.Exists) {
            
            $fileToLoad = [IO.FileInfo](Join-Path (Get-Location) $fileToLoad.Name)
        }
        
        if (!$fileToLoad.Exists) {
        
            Throw "'$($fileToLoad.FullName)' does not exist."
        }
        
        return ([FileHashLookup] (Import-Clixml -Path $fileToLoad))
    }

    [FileHashLookup] GetDiffInOther([FileHashLookup] $other) { 

        # Powershell does not (yet) support optional args in class methods.. :(
        return $this.Compare($other, $true, $false)
    }

    [FileHashLookup] GetMatchesInOther([FileHashLookup] $other) { 
        # Powershell does not (yet) support optional args in class methods.. :(
        return $this.Compare($other, $false, $true)
    }

    AddFileHashTable([FileHashLookup] $other) {
    
        $sw = [Diagnostics.Stopwatch]::StartNew()

        $files = @($other.File.Keys)

        for($i = 0; $i -lt $other.File.Count; $i++ )
        {
            $currentFile = $files[$i]
            $currentHash = $other.File.($currentFile)

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Adding..." -Status "($i of $($other.File.Count)) $currentFile" -PercentComple ($i / $other.File.Count * 100)
                $sw.Restart()
            }
            
            $this.Add($currentFile, $currentHash)		
        }
    } 

    ExcludeFilePattern ([string] $filePattern) {
        
        $this.ExcludedFilePatterns.Add($filePattern) > $null

        $filesToRemove = $this.GetFiles() | ?{ $file = $_; ( $this.ExcludedFilePatterns | ?{ $file -ne $null -and $file.Name -like $_ } ) -ne $null } 

        $sw = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $filesToRemove.Count; $i++ ) {
        
            $currentFile = $filesToRemove[$i]

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Removing from FileHashTable..." -Status "($i of $($filesToRemove.Count)) $($currentFile.FullName)" -PercentComple ($i / $filesToRemove.Count * 100)
                $sw.Restart()
            }

            $this.Remove($currentFile)
        }
    }

    ExcludeFolder ([IO.DirectoryInfo] $folder) {
    
        $this.ExcludedFolders.Add($folder.FullName) > $null

        $filesToRemove = $this.GetFiles() | ?{ $file = $_; ($this.ExcludedFolders | ?{ $file -ne $null -and $file.FullName.StartsWith($_) }) -ne $null }

        $sw = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $filesToRemove.Count; $i++ ) {
        
            $currentFile = $filesToRemove[$i]

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Removing from FileHashTable..." -Status "($i of $($filesToRemove.Count)) $($currentFile.FullName)" -PercentComple ($i / $filesToRemove.Count * 100)
                $sw.Restart()
            }

            $this.Remove($currentFile)
        }
    }

    hidden Add ([IO.FileInfo] $file) {

        $this.Add($file, $null)
    }

    hidden Add ([IO.FileInfo] $file, [string] $hash) {

        $fileName = $file.FullName

        $fileHash = if (!$hash) { (Get-FileHash -LiteralPath $fileName -Algorithm MD5 -ErrorAction Continue).Hash } else { $hash }

        if ($fileHash) {

            if (($this.Hash.ContainsKey($fileHash)) -and (!$this.Hash.($fileHash).Contains($fileName))) {
                $this.Hash.($fileHash).Add($fileName)
            }
            else {
                $this.Hash.($fileHash) = [Collections.ArrayList] @($fileName)
            }
            
            $this.File.($fileName) = $fileHash
        }
    }

    hidden Remove ([IO.FileInfo] $file) {
    
        $fileName = $file.FullName
        
        if ($this.File.ContainsKey($fileName)) {
            
            $fileHash = $this.File.($fileName)
            
            $this.File.Remove($fileName)
            
            $this.Hash.($fileHash).Remove($fileName)

            if ($this.Hash.($fileHash).Count -eq 0) {
                $this.Hash.Remove($fileHash)
            }
        }
    }

    hidden [FileHashLookup] Compare([FileHashLookup] $other, [switch] $getDifferences = $false, [switch] $getMatches = $false) {
        
        Write-Progress -Activity "Comparing files" -Status "Analyzing differences..."
        
        $newLookup = [FileHashLookup]::New()

        $sw = [Diagnostics.Stopwatch]::StartNew()
        
        $otherFiles = $other.GetFiles()
        
        for($i = 0; $i -lt $other.File.Count; $i++) {
            
            $currentFile = $otherFiles[$i].FullName
            $currentHash = $other.File.($currentFile)
            
            if ($sw.Elapsed.TotalMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Comparing hashes" -Status ("Processing {0}" -f $currentFile) -PercentComplete ($i / $other.File.Count * 100) 
                $sw.Restart()
            }

            if ($getDifferences -and !$this.Hash.ContainsKey($currentHash)) {
                
                $newLookup.Add($currentFile, $currentHash);
            }

            if ($getMatches -and $this.Hash.ContainsKey($currentHash)) {
                
                $newLookup.Add($currentFile, $currentHash);
            }
        }

        return $newLookup
    }

    [string] ToString() 
    {
        $msg = if ($this.File.Keys.Count -eq 0) { "`nFileHashTable is empty." } else { "`nFileHashTable contains $($this.File.Keys.Count) files." } 
    
        if ($this.Paths) {
            $msg += "`n`nMonitored Folders: `n`n" + (($this.Paths | %{ "  > $_"} ) -join "`r`n")
        }
    
        if (($this.ExcludedFilePatterns + $this.ExcludedFolders).Count -gt 0) {
            $msg += "`n`nExcluded Folders and file patterns: `n`n" + ((($this.ExcludedFilePatterns + $this.ExcludedFolders) | %{ "  > $_"} ) -join "`r`n")
        }
        
        if ($this.SavedAsFile) {
            $msg += "`n`nLast saved: $($this.SavedAsFile)" 
        }

        $msg += "`n`nLast updated: $($this.LastUpdated.ToString("dd-MM-yyyy HH:mm:ss"))`n" 
                
        return $msg
    }
}