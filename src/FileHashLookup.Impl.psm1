using module .\BasicFileInfo.psm1
using namespace System.Collections.Generic;

class FileHashLookup 
{
    FileHashLookup() 
    {
        $this.Init($null)
    }
    
    FileHashLookup([IO.DirectoryInfo] $path)
    {
        $this.Init($path)
    }

    hidden [void] Init([IO.DirectoryInfo] $path)
    {
        $this.File = [Dictionary[string, BasicFileInfo]]@{}
        $this.Hash = [Dictionary[string, List[BasicFileInfo]]]@{}
        $this.ExcludedFilePatterns = [List[string]]@()
        $this.IncludedFilePatterns = [List[string]]@()
        $this.ExcludedFolders = [List[string]]@()
        $this.Paths = [List[string]]@()

        if ($path) {

            $absolutePath = [IO.DirectoryInfo](GetAbsolutePath $path)

            $replaceableChars = ([IO.Path]::GetInvalidFileNameChars() + ' ' | Foreach-Object { [Regex]::Escape($_) }) -join "|"
            $fileName = "$($absolutePath.FullName -replace $replaceableChars, "_").xml"  

            $this.SavedAsFile = (GetAbsolutePath $fileName)

            $this.AddFolder($absolutePath)

            $this.Save()
        }

        $this.LastUpdated = Get-Date
    }

    hidden [Dictionary[string, BasicFileInfo]] $File
    hidden [Dictionary[string, List[BasicFileInfo]]] $Hash
    hidden [List[string]] $Paths
    hidden [List[string]] $ExcludedFilePatterns
    hidden [List[string]] $IncludedFilePatterns
    hidden [List[string]] $ExcludedFolders
    hidden [DateTime] $LastUpdated
    hidden [string] $SavedAsFile

    [IO.FileInfo[]] GetFiles() {
        
        return $this.File.Keys | Sort-Object -prop FullName | Foreach-Object { [IO.FileInfo]$_ }
    }

    [IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file) {

        $fileForHash = [IO.FileInfo](GetAbsolutePath $file)
        
        $fileHash = $this.File.($fileForHash).Hash ?? ([BasicFileInfo]::New($fileForHash)).Hash   
        
        return $this.Hash.($fileHash) | Sort-Object -prop FullName | Foreach-Object { [IO.FileInfo]$_ }
    }

    [bool] Contains([IO.FileInfo] $file) {
        
        return $this.File.ContainsKey((GetAbsolutePath $file))
    }

    hidden Add ([IO.FileInfo] $file) {

        $this.Add($file, $null)
    }

    hidden Add ([IO.FileInfo] $file, [string] $hash) {

        $this.Add([BasicFileInfo]::new($file, $hash))
    }
   
    hidden Add ([BasicFileInfo] $newFile) {

        if ($newFile.Hash) {

            if (!$this.Hash.ContainsKey($newFile.Hash)) {
            
                $this.Hash.($newFile.Hash) = [List[BasicFileInfo]]@($newFile)
            }
    
            if ($this.Hash.($newFile.Hash) -notcontains $newFile) {
                
                $this.Hash.($newFile.Hash).Add($newFile)
            }
            
            $this.File.($newFile.FullName) = $newFile
        }
    }

    hidden Remove ([IO.FileInfo] $file) {
    
        if ($this.File.ContainsKey($file.FullName)) {
            
            $fileToRemove = $this.File.($file.FullName)
            
            $this.File.Remove($fileToRemove.FullName)
            
            $this.Hash.($fileToRemove.Hash).Remove($fileToRemove)

            if ($this.Hash.($fileToRemove.Hash).Count -eq 0) {

                $this.Hash.Remove($fileToRemove.Hash)
            }
        }
    }
 

    AddFolder([IO.DirectoryInfo] $path) {
        
        $path = [IO.DirectoryInfo](GetAbsolutePath $path)
        
        Write-Progress -Activity "Adding or updating files" -Status "Collecting files..."

        $files = @()
        $filesToExclude = @()

        if ($path.Exists) {
        
            if (!($this.Paths -contains $path.FullName)) {
            
                $this.Paths.Add($path.FullName)
            }

            $getChildItemArgs = @{

                Path = $path.FullName;
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
                $files = $files | Where-Object { $file = $_; $null -eq ($applicableExcludedFolders | Where-Object { $file.FullName.StartsWith($_) }) }

                # Remove files which were added once, and are now located in excluded folders.
                $filesToExclude = $this.GetFiles() | Where-Object { $file = $_; $null -ne ($applicableExcludedFolders | Where-Object { $null -ne $file -and $file.FullName.StartsWith($_) }) } 
            }
        }

        Write-Progress -Activity "Adding or updating files" -Status "Detecting modified files..."

        # Later modified files should be refreshed
        $files | Where-Object { $this.Contains($_) -and $this.File.($_) -lt $_ } | Foreach-Object { $this.Remove($_) }

        Write-Progress -Activity "Adding or updating files" -Status "Analyzing differences..."
         
        $newlyAddedFiles = $files | Where-Object { !$this.Contains($_) } | Foreach-Object { @{ Operation='Add'; File=$_ } }
        $deletedFiles = $this.GetFiles() | Where-Object {$_ -ne $null -and !$_.Exists } | Foreach-Object { @{ Operation='Remove'; File=$_ } }
        $excludedFiles = $filesToExclude | Foreach-Object { @{ Operation='Remove'; File=$_  } }
        
        $itemsToUpdate = @($newlyAddedFiles) + @($deletedFiles) + @($excludedFiles)

        $updateStatusTimer = [Diagnostics.Stopwatch]::StartNew()
        $saveProgressTimer =  [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $itemsToUpdate.Count; $i++ ) {
        
            $currentFile = $itemsToUpdate[$i].('File')
            $currentOperation = $itemsToUpdate[$i].('Operation')

            if ($updateStatusTimer.ElapsedMilliseconds -ge 500) 
            {

                $activity = $currentOperation -eq "Add" ?  "Calculating Hash..." : "Removing from FileHashTable..."	        
                Write-Progress -Activity $activity -Status "($i of $($itemsToUpdate.Count)) $($currentFile.FullName)" -PercentComplete ($i / $itemsToUpdate.Count * 100)
                $updateStatusTimer.Restart()
            }
            
            if ($saveProgressTimer.Elapsed -ge ([Timespan]::FromMinutes(15)))
            {
                $this.Save()
                $saveProgressTimer.Restart()
            }

            if ($currentOperation -eq "Add") 
            {
                $this.Add($currentFile)    
            } 
            elseif ($currentOperation -eq "Remove") 
            {
                $this.Remove($currentFile)
            }
        }
    }
	    
    Refresh() {

        $this.Paths | ForEach-Object { $this.AddFolder(($_)) }

        $this.Paths = [List[string]]@(($this.Paths | Where-Object { ([IO.DirectoryInfo]$_).Exists }))

        if (!($this.Paths)) {
        
            Write-Progress -Activity "Refresh: Collecting files to refresh..."

            $files = $this.GetFiles() | Where-Object { $_ -ne $null }

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
    }
   
    AddFileHashTable([FileHashLookup] $other) {
    
        $sw = [Diagnostics.Stopwatch]::StartNew()

        $files = @($other.File.Values)

        for($i = 0; $i -lt $other.File.Count; $i++ )
        {
            $currentFile = $files[$i]

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Adding..." -Status "($i of $($other.File.Count)) $($currentFile.FullName)" -PercentComple ($i / $other.File.Count * 100)
                $sw.Restart()
            }
            
            $this.Add($currentFile.FullName, $currentFile.Hash)		
        }

        $this.Paths = [List[string]]@(@($this.Paths) + @($other.Paths) | Select-Object -Unique) 
    } 

    IncludeFilePattern ([string] $filePattern) { 

        $this.IncludedFilePatterns.Add($filePattern)
    }
    
    ExcludeFilePattern ([string] $filePattern) {
        
        $this.ExcludedFilePatterns.Add($filePattern)

        $filesToRemove = $this.GetFiles() | Where-Object { $file = $_; $null -ne ( $this.ExcludedFilePatterns | Where-Object { $file.Name -like $_ } ) } 

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
        
        $this.ExcludedFolders.Add($folder.FullName)

        $this.Refresh()
    }
    
    Refresh() {

        $this.Paths | ForEach-Object { $this.AddFolder(($_)) }

        $this.Paths = [List[string]]@(($this.Paths | Where-Object { ([IO.DirectoryInfo]$_).Exists }))

        Write-Progress -Activity "Refresh: Collecting files to refresh..."

        $files = $this.GetFiles() | Where-Object { $_ -ne $null }

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

            $this.SavedAsFile = (New-TemporaryFile).FullName
        } 
    
        New-Item -ItemType File $this.SavedAsFile -Force
        Export-Clixml -Path $this.SavedAsFile -InputObject $this        
    }
    
    static [FileHashLookup] Load([IO.FileInfo] $fileToLoad) {
    
        $fileToLoad = [IO.FileInfo](GetAbsolutePath $fileToLoad)
        
        if (!$fileToLoad.Exists) {
        
            Throw "'$($fileToLoad.FullName)' does not exist."
        }

        $deserialized = Import-Clixml -Path $fileToLoad
        $newLookup = [FileHashLookup]::new()

        $newLookup.File = [Dictionary[string, BasicFileInfo]] [List[KeyValuePair[string, BasicFileInfo]]] `
            ($deserialized.File.GetEnumerator() | ForEach-Object {  `
            [KeyValuePair[string, BasicFileInfo]]::new($_.Name, [BasicFileInfo]$_.Value) })

        $newLookup.Hash = [Dictionary[string, [List[BasicFileInfo]]]] [List[KeyValuePair[string, List[BasicFileInfo]]]] `
            ($deserialized.Hash.GetEnumerator() | ForEach-Object { `
             $entries = ($_.Value | ForEach-Object{ [BasicFileInfo]$_.Value });
            [KeyValuePair[string, List[BasicFileInfo]]]::new($_.Name, ([List[BasicFileInfo]] $entries ))})
        
        $newLookup.ExcludedFilePatterns = $deserialized.ExcludedFilePatterns
        $newLookup.IncludedFilePatterns = $deserialized.IncludedFilePatterns
        $newLookup.ExcludedFolders = $deserialized.ExcludedFolders
        $newLookup.LastUpdated = $deserialized.LastUpdated
        $newLookup.SavedAsFile = $deserialized.SavedAsFile

        return $newLookup
    }

    AddFileHashTable([FileHashLookup] $other) {
    
        $sw = [Diagnostics.Stopwatch]::StartNew()

        $files = @($other.File.Values)

        for($i = 0; $i -lt $other.File.Count; $i++ )
        {
            $currentFile = $files[$i]

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Adding..." -Status "($i of $($other.File.Count)) $($currentFile.FullName)" -PercentComple ($i / $other.File.Count * 100)
                $sw.Restart()
            }
            
            $this.Add($currentFile.FullName, $currentFile.Hash)		
        }

        $this.Paths = [List[string]]@(@($this.Paths) + @($other.Paths) | Select-Object -Unique) 
    } 

    [FileHashLookup] GetDiffInOther([FileHashLookup] $other) { 

        return $this.Compare($other).Differences
    }

    [FileHashLookup] GetMatchesInOther([FileHashLookup] $other) { 

        return $this.Compare($other).Matches
    }

    hidden [PsCustomObject] Compare([FileHashLookup] $other) {
        
        Write-Progress -Activity "Comparing files" -Status "Analyzing differences..."
        
        $differencesLookup = [FileHashLookup]::New() 
        $matchesLookup = [FileHashLookup]::New() 

        $sw = [Diagnostics.Stopwatch]::StartNew()
        
        $otherFiles = @($other.File.Values.GetEnumerator()) 
        
        for($i = 0; $i -lt $otherFiles.Count; $i++) {
            
            $currentFile = $otherFiles[$i]
            
            if ($sw.Elapsed.TotalMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Comparing hashes" -Status "($i of $($otherFiles.Count)) Processing $($currentFile.FullName)" -PercentComplete ($i / $otherFiles.Count * 100)
                $sw.Restart()
            }

            if ($this.Hash.ContainsKey($currentFile.Hash))  {
                 $matchesLookup.Add($currentFile)
            }
            else {
                $differencesLookup.Add($currentFile)
            }
        }

        return [PsCustomObject]@{ Matches=$matchesLookup; Differences=$differencesLookup; }
    }

    [string] ToString() 
    {
        $msg = [Text.StringBuilder]::new($this.File.Keys.Count -eq 0 `
            ? "`nFileHashTable is empty.`n" `
            : "`nFileHashTable contains $($this.File.Keys.Count) files.`n")
            
        $msg.AppendLine($this.Paths.Count -gt 0 `
            ? "`nMonitored Folders: `n`n" + (($this.Paths | Foreach-Object { "  > $_"} ) -join "`r`n") `
            : "`nMonitored Folders: <none>") 
    
        if ($this.IncludedFilePatterns.Count -gt 0) {
            $msg.AppendLine("`nIncluded file patterns: `n" + ((($this.IncludedFilePatterns) | Foreach-Object { "  > $_"} ) -join "`r`n"))
        }
        
        if (($this.ExcludedFilePatterns + $this.ExcludedFolders).Count -gt 0) {
            $excludedItems = ((($this.ExcludedFilePatterns + $this.ExcludedFolders) | Foreach-Object { "  > $_"} ) -join "`r`n")
            $msg.AppendLine("`nExcluded Folders and file patterns: `n" + $excludedItems) 
        }
        
        if ($this.SavedAsFile) {
            $msg.AppendLine("`nLast saved: $($this.SavedAsFile)")
        }
    
        $msg.AppendLine("`nLast updated: $($this.LastUpdated.ToString("dd-MM-yyyy HH:mm:ss"))`n") 
                
        return $msg.ToString()
    }
}

function GetAbsolutePath {
    param([string] $path)

    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
}