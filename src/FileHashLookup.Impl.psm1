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
        $this.Paths = [List[string]]@{}

        if ($path) {

            $absolutePath = [IO.DirectoryInfo](GetAbsolutePath $path)
            $this.AddFolder($absolutePath)

            $replaceableChars = (([IO.Path]::GetInvalidFileNameChars() + ' ' | Foreach-Object { [Regex]::Escape($_) }) -join "|")
            $fileName = "$($absolutePath.FullName -replace $replaceableChars, "_").xml"  

            $this.Save((GetAbsolutePath $fileName))
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
        
        return $this.File.Keys | Sort-Object | Foreach-Object { [IO.FileInfo] $_ }
    }

    [IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file) {

        $fileForHash = [IO.FileInfo](GetAbsolutePath $file)
        
        $fileHash = $this.File.($fileForHash.FullName).Hash ?? ([BasicFileInfo]::New($fileForHash)).Hash   
        
        return $this.Hash.($fileHash) | Sort-Object | Foreach-Object { [IO.FileInfo] $_.FullName }
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
                $filesToExclude = $this.GetFiles() | Where-Object { $_ -ne $null } | Where-Object { $file = $_; $null -ne ($applicableExcludedFolders | Where-Object { $file.FullName.StartsWith($_) }) } 
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

        $deserialized = Import-Clixml -Path $fileToLoad
        $newLookup = [FileHashLookup]::new()

        $newLookup.File = [Dictionary[string, BasicFileInfo]] [List[KeyValuePair[string, BasicFileInfo]]] `
            ($deserialized.File.GetEnumerator() | ForEach-Object {  `
            [KeyValuePair[string, BasicFileInfo]]::new($_.Name, [BasicFileInfo]$_.Value) })

        $newLookup.Hash = [Dictionary[string, [List[BasicFileInfo]]]] [List[KeyValuePair[string, List[BasicFileInfo]]]] `
            ($deserialized.Hash.GetEnumerator() | ForEach-Object { `
             $entries = ($_.Value | ForEach-Object{ [BasicFileInfo]$_.Value });
            [KeyValuePair[string, List[BasicFileInfo]]]::new($_.Name, ([List[BasicFileInfo]] $entries ))})
        
        $newLookup.Paths = $deserialized.Paths
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

    IncludeFilePattern ([string] $filePattern) { 

        $this.IncludedFilePatterns.Add($filePattern)
    }
    
    ExcludeFilePattern ([string] $filePattern) {
        
        $this.ExcludedFilePatterns.Add($filePattern)

        $filesToRemove = $this.GetFiles() | Where-Object { $file = $_; $null -ne ( $this.ExcludedFilePatterns | Where-Object { $null -ne $file -and $file.Name -like $_ } ) } 

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
        
        $otherFiles = $other.File.Values.GetEnumerator() | ForEach-Object { $_ } 
        
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

class BasicFileInfo : IComparable
{
    BasicFileInfo()
    {
    }
    
    BasicFileInfo([IO.FileInfo] $file)
    {
        $this.Init($file, $null)
    }

    BasicFileInfo([IO.FileInfo] $file, [string] $hash)
    {
        $this.Init($file, $hash)
    }

    hidden [void] Init([IO.FileInfo] $file, [string] $hash)
    {
        $this.FullName = $file.FullName
        $this.CreationTime = $file.CreationTime
        $this.LastWriteTime = $file.LastWriteTime

        # Null coalescing doesn't work here because passing $null as a string becomes "" for $hash :(
        $this.Hash = ![string]::IsNullOrEmpty($hash) ? $hash : (Get-FileHash -LiteralPath $file -Algorithm MD5 -ErrorAction SilentlyContinue).Hash
    }

    [string] $FullName
    [DateTime] $CreationTime
    [DateTime] $LastWriteTime
    [string] $Hash

    [bool] Equals ($that) 
    {
        if (@([IO.FileInfo], [BasicFileInfo]) -notcontains $that.GetType()) {
            return $false
        }

        $isEqual = $this.FullName -eq $that.FullName -and $this.CreationTime -eq $that.CreationTime -and $this.LastWriteTime -eq $that.LastWriteTime

        return $that -is [BasicFileInfo] `
            ? $isEqual -and $this.Hash -eq $that.Hash `
            : $isEqual
    }    

    [int] CompareTo($that)
    {
        if (@([IO.FileInfo], [BasicFileInfo]) -notcontains $that.GetType()) {
            Throw "Can't compare"
        }
        
        return (($this.CreationTime - $that.CreationTime) + ($this.LastWriteTime - $that.LastWriteTime)).TotalSeconds  
    }    

    static [IO.FileInfo] op_Implicit([BasicFileInfo] $instance) {
        
        return [IO.FileInfo] $instance.FullName
    }
}

function GetAbsolutePath {
    param([string] $path)

    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
}