class FileHashLookup 
{
    FileHashLookup() 
    {
        $this.File = @{}
        $this.Hash = @{}
        $this.Paths = [Collections.ArrayList]@()
        $this.ExcludedFilePatterns = [Collections.ArrayList]@()
        $this.IncludedFilePatterns = [Collections.ArrayList]@()
        $this.ExcludedFolders = [Collections.ArrayList]@()

        $this.LastUpdated = Get-Date
    }

    FileHashLookup([IO.DirectoryInfo] $path)
    {
        $absolutePath = [IO.DirectoryInfo](GetAbsolutePath $path)
        
        $this.File = @{}
        $this.Hash = @{}
        $this.Paths = [Collections.ArrayList]@($absolutePath.FullName)
        $this.ExcludedFilePatterns = [Collections.ArrayList]@()
        $this.IncludedFilePatterns = [Collections.ArrayList]@()
        $this.ExcludedFolders = [Collections.ArrayList]@()

        $this.AddFolder($absolutePath)
        $this.LastUpdated = Get-Date

        $fileName = ($absolutePath.FullName -replace (([IO.Path]::GetInvalidFileNameChars() + ' ' | Foreach-Object { [Regex]::Escape($_) }) -join "|"), "_") + ".xml"  
        
        $this.Save((GetAbsolutePath $fileName))
    }

    hidden [HashTable] $File
    hidden [HashTable] $Hash
    hidden [Collections.ArrayList] $Paths
    hidden [Collections.ArrayList] $ExcludedFilePatterns
    hidden [Collections.ArrayList] $IncludedFilePatterns
    hidden [Collections.ArrayList] $ExcludedFolders
    hidden [DateTime] $LastUpdated
    hidden [string] $SavedAsFile

    [IO.FileInfo[]] GetFiles() {
        
        return $this.File.Keys | Sort-Object | Foreach-Object { [IO.FileInfo] $_ }
    }

    [IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file) {
        
        $fileForHash = [IO.FileInfo](GetAbsolutePath $file)
        
        if ($this.Contains($fileForHash))
        {
            $fileHash = $this.File.($fileForHash.FullName)
        } 
        else 
        {
            $fileHash = (Get-FileHash -LiteralPath $file -Algorithm MD5).Hash
        }
        
        return $this.Hash.($fileHash) | Sort-Object | Foreach-Object { [IO.FileInfo] $_ }
    }

    [bool] Contains([IO.FileInfo] $file) {
        
        return $this.File.ContainsKey((GetAbsolutePath $file))
    }   

    AddFolder([IO.DirectoryInfo] $path) {
        
        $path = [IO.DirectoryInfo](GetAbsolutePath $path)
        
        Write-Progress -Activity "Adding or updating files" -Status "Collecting files..."

        $files = @()
        $filesToExclude = @()

        if ($path.Exists) {
        
            if (!($this.Paths -contains $path.FullName)) {
            
                $this.Paths.Add($path.FullName) > $null
            }

            $getChildItemArgs = @{

                Path = "$($path.FullName)";
                File = $true;
                Recurse = $true;
                Force = $true;
                ErrorAction = "SilentlyContinue";
            }

            if ($this.ExcludedFilePatterns) { $getChildItemArgs.Add("Exclude", $this.ExcludedFilePatterns) }
            if ($this.IncludedFilePatterns) { $getChildItemArgs.Add("Include", $this.IncludedFilePatterns) }

            $files = Get-ChildItem @getChildItemArgs

            $applicableExcludedFolders = $this.ExcludedFolders | Where-Object { $_.StartsWith(($path.FullName)) }

            if ($applicableExcludedFolders) {
        
                # Dont add Files from path which are located in excluded folders                
                $files = $files | Where-Object { $file = $_; ($applicableExcludedFolders | Where-Object { $file.FullName.StartsWith($_) }) -eq $null }

                # Remove files which were added once, and are now located in excluded folders.
                $filesToExclude = $this.GetFiles() | Where-Object { $_ -ne $null } | Where-Object { $file = $_; ($applicableExcludedFolders | Where-Object { $file.FullName.StartsWith($_) }) } 
            }
        }

        Write-Progress -Activity "Adding or updating files" -Status "Detecting modified files..."

        $files | Where-Object { $_.LastWriteTime -gt $this.LastUpdated } | Foreach-Object { $this.Remove($_) }

        Write-Progress -Activity "Adding or updating files" -Status "Analyzing differences..."
         
        $newlyAddedFiles = $files | Where-Object { !$this.Contains($_.FullName) } | Foreach-Object { @{ Operation='Add'; File=$_ } }
        $deletedFiles = $this.GetFiles() | Where-Object { $_ -ne $null -and !$_.Exists } | Foreach-Object { @{ Operation='Remove'; File=$_  } }
        $excludedFiles = $filesToExclude | Where-Object { $_ -ne $null } | Foreach-Object { @{ Operation='Remove'; File=$_  } }
        
        $itemsToUpdate = @($newlyAddedFiles) + @($deletedFiles) + @($excludedFiles)

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

        $this.Paths | ForEach-Object { $this.AddFolder(($_)) }

        $this.Paths = [Collections.ArrayList]@(($this.Paths | Where-Object { ([IO.DirectoryInfo]$_).Exists }))

        if (!($this.Paths)) {
        
            Write-Progress -Activity "Refresh: Collecting files to refresh..."

            $files = $this.GetFiles() | ?{ $_ -ne $null }

            $sw = [Diagnostics.Stopwatch]::StartNew()
            
            for($i = 0; $i -lt $files.Count; $i++ )
            {
                $currentFile = $files[$i]
                
                if ($sw.ElapsedMilliseconds -ge 500) 
                {
                    Write-Progress -Activity "Refresh: Remove files which no longer exists..." -Status "($i of $($files.Count)) $($currentFile.FullName)" -PercentComple ($i / $files.Count * 100)
                    $sw.Restart()
                }

                if (!($currentFile.Exists)) {
                
                    $this.Remove($currentFile)
                }
            }
        }

        $this.LastUpdated = Get-Date
    }

    Save([IO.FileInfo] $filename) {
    
        if ($filename) {
        
            $this.SavedAsFile = (GetAbsolutePath $filename)
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
    
        $fileToLoad = [IO.FileInfo](GetAbsolutePath $fileToLoad)
        
        if (!$fileToLoad.Exists) {
        
            Throw "'$($fileToLoad.FullName)' does not exist."
        }
        
        return ([FileHashLookup] (Import-Clixml -Path $fileToLoad))
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

        $this.Paths = [Collections.ArrayList]@(@($this.Paths) + @($other.Paths) | Select -Unique) 
    } 

    IncludeFilePattern ([string] $filePattern) { 

        $this.IncludedFilePatterns.Add($filePattern) > $null
    }
    
    ExcludeFilePattern ([string] $filePattern) {
        
        $this.ExcludedFilePatterns.Add($filePattern) > $null

        $filesToRemove = $this.GetFiles() | Where-Object { $file = $_; ( $this.ExcludedFilePatterns | Where-Object { $file -ne $null -and $file.Name -like $_ } ) -ne $null } 

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
    
        $folder = [IO.DirectoryInfo] (GetAbsolutePath $folder)
        
        $this.ExcludedFolders.Add($folder.FullName) > $null

        $this.Refresh()
    }

    hidden Add ([IO.FileInfo] $file) {

        $this.Add($file, $null)
    }

    hidden Add ([IO.FileInfo] $file, [string] $hash) {

        $fileName = $file.FullName

        $fileHash = if (!$hash) { (Get-FileHash -LiteralPath $fileName -Algorithm MD5 -ErrorAction Continue).Hash } else { $hash }

        if ($fileHash) {
            
            if (!$this.Hash.ContainsKey($fileHash)) {
            
                $this.Hash.($fileHash) = [Collections.ArrayList] @($fileName)
            }
            
            if ((!$this.Hash.($fileHash).Contains($fileName))) {
                
                $this.Hash.($fileHash).Add($fileName)
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

    [FileHashLookup] GetDiffInOther([FileHashLookup] $other) { 

        # Powershell does not (yet) support optional args in class methods.. :(
        return $this.Compare($other, $true, $false)
    }

    [FileHashLookup] GetMatchesInOther([FileHashLookup] $other) { 
        # Powershell does not (yet) support optional args in class methods.. :(
        return $this.Compare($other, $false, $true)
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
                Write-Progress -Activity "Comparing hashes" -Status ("($i of $($other.File.Count)) Processing {0}" -f $currentFile) -PercentComplete ($i / $other.File.Count * 100)
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

    [FileHashLookup] GetDuplicates() { 

        $sw = [Diagnostics.Stopwatch]::StartNew()

        $duplicateFiles = [FileHashLookup]::new()

        $hashEntries = $this.Hash.GetEnumerator() | Foreach-Object { $_ }

        for($i = 0; $i -lt $hashEntries.Count; $i++) {

            $values = @($hashEntries[$i].Value)
            
            if ($values.Count -gt 1) {
            
                $values | Foreach-Object { $duplicateFiles.Add($_) }
            }

            if ($sw.Elapsed.TotalMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Selecting duplicates" -Status ("($i of $($hashEntries.Count)) Processing {0}" -f $values[0]) -PercentComplete  ($i / $hashEntries.Count * 100)
                $sw.Restart()
            }
        }

        return $duplicateFiles
    }

    [string] ToString() 
    {
        $msg = if ($this.File.Keys.Count -eq 0) { "`nFileHashTable is empty." } else { "`nFileHashTable contains $($this.File.Keys.Count) files." } 
    
        $msg += if ($this.Paths.Count -gt 0) { "`n`nMonitored Folders: `n`n" + (($this.Paths | Foreach-Object { "  > $_"} ) -join "`r`n") } else { "`n`nMonitored Folders: <none>" }
    
        if ($this.IncludedFilePatterns.Count -gt 0) {
            $msg += "`n`nIncluded file patterns: `n" + ((($this.IncludedFilePatterns) | Foreach-Object { "  > $_"} ) -join "`r`n")
        }
        
        if (($this.ExcludedFilePatterns + $this.ExcludedFolders).Count -gt 0) {
            $msg += "`n`nExcluded Folders and file patterns: `n" + ((($this.ExcludedFilePatterns + $this.ExcludedFolders) | Foreach-Object { "  > $_"} ) -join "`r`n")
        }
        
        if ($this.SavedAsFile) {
            $msg += "`n`nLast saved: $($this.SavedAsFile)" 
        }
    
        $msg += "`n`nLast updated: $($this.LastUpdated.ToString("dd-MM-yyyy HH:mm:ss"))`n" 
                
        return $msg
    }
}

function GetAbsolutePath {
    param([string] $path)

    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
}