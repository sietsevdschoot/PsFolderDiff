using module .\BasicFileInfo.psm1
using namespace System.Collections.Generic;
using namespace System.Text.RegularExpressions;

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
        
        return $this.File.Keys.ForEach({ [IO.FileInfo]$_ })
    }

    [IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file) {

        $fileHash = $this.File.($file.FullName).Hash ?? ([BasicFileInfo]::New($file)).Hash   
        
        return $this.Hash.($fileHash) | ForEach-Object { [IO.FileInfo]$_ }
    }

    [bool] Contains([IO.FileInfo] $file) {
        
        return $this.File.ContainsKey((GetAbsolutePath $file))
    }

    Add ([IO.FileInfo] $file) {

        $this.Add($file, $null)
    }

    hidden Add ([IO.FileInfo] $file, [string] $hash) {

        $this.Add([BasicFileInfo]::new($file, $hash))
    }
   
    hidden Add ([BasicFileInfo] $newFile) {

        # Hash needs to be provided or calculated. 
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

        if ($this.Paths -notcontains $path.FullName) {
            
            $this.Paths.Add($path.FullName)
        }

        $files = $this.InternalGetFiles($path)

        $itemsToUpdate = $this.InternalGetFilesToAddOrUpdate($files)

        $this.InternalApplyChanges($itemsToUpdate)

        $this.LastUpdated = Get-Date
    }

    AddFileHashTable([FileHashLookup] $other) {
    
        $updateStatusTimer = [Diagnostics.Stopwatch]::StartNew()
        $durationTimer = [Diagnostics.Stopwatch]::StartNew()

        $files = @($other.File.Values.GetEnumerator())

        for($i = 0; $i -lt $other.File.Count; $i++ )
        {
            $currentFile = $files[$i]

            if ($updateStatusTimer.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Adding..." `
                    -Status "($i of $($other.File.Count)) $($currentFile.FullName)" `
                    -PercentComple ($i / $other.File.Count * 100) `
                    -SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($other.File.Count - $i)))
                
                $updateStatusTimer.Restart()
            }
            
            $this.Add($currentFile.FullName, $currentFile.Hash)		
        }

        $this.Paths = [List[string]]@(@($this.Paths) + @($other.Paths) | Select-Object -Unique) 
    } 

    IncludeFilePattern ([string] $filePattern) { 

        $this.IncludedFilePatterns.Add($filePattern)
    }
    
    ExcludeFilePattern ([string] $filePattern) {
        
        try { [Regex]::new($this.CreateExcludedFilePatternRegexString(($this.ExcludedFilePatterns + @($filePattern)))) }
        catch { Throw "'$filePattern' is not valid. `n$($_.Exception.Message)" }
        
        $this.ExcludedFilePatterns.Add($filePattern)
    }

    ExcludeFolder ([IO.DirectoryInfo] $folder) {
    
        $folder = [IO.DirectoryInfo] (GetAbsolutePath $folder)

        try { [Regex]::new($this.CreateExcludedFilePathRegexString(($this.ExcludedFolders + @($folder.FullName)))) }
        catch { Throw "'$($folder.FullName)' is not valid. `n$($_.Exception.Message)" }

        $this.ExcludedFolders.Add($folder.FullName)
    }
    
    Refresh() {

        Write-Progress -Activity "Refresh: Collecting files to refresh..."

        $collectedFiles = [List[IO.FileInfo]]@()
        $itemsToUpdate = [List[PsCustomObject]]@()

        foreach ($path in $this.Paths) 
        {
            $files = $this.InternalGetFiles($path)
            $collectedFiles.AddRange($files)
            
            $updatedItems = $this.InternalGetFilesToAddOrUpdate($files)
            $itemsToUpdate.AddRange($updatedItems)
        }

        $trackedFiles = $this.GetFiles()
    
        # Checking deleted tracked files
        $collectedFilesLookup = [HashSet[IO.FileInfo]]::new($collectedFiles)
        $trackedFilesToRefresh = $trackedFiles.Where({!$collectedFilesLookup.Contains($_)})

        $updateStatusTimer = [Diagnostics.Stopwatch]::StartNew()
        $durationTimer = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $trackedFilesToRefresh.Count; $i++) {

            $currentFile = $trackedFilesToRefresh[$i]

            # Tracked file that is now deleted.
            if (!$collectedFilesLookup.Contains($currentFile) -and !$currentFile.Exists) {

                $itemsToUpdate.Add(([PsCustomObject]@{ Operation='Remove'; File=$currentFile }))
            }

            if ($updateStatusTimer.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Refresh: Adding or updating files" `
                    -Status "Checking deleted files..." `
                    -PercentComplete (($i / $trackedFilesToRefresh.Count) * 100) `
                    -CurrentOperation "($i of $($trackedFilesToRefresh.Count)) $($currentFile.FullName)" `
                    -SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($trackedFilesToRefresh.Count - $i)))

                $updateStatusTimer.Restart()
            }
        }

        if ($this.ExcludedFolders -or $this.ExcludedFilePatterns) {

            Write-Progress -Activity "Refresh: Checking files to exclude..."

            $excludePathRegexStr = $this.CreateExcludedFilePathRegexString($this.ExcludedFolders)
            $excludePathRegex = [Regex]::new($excludePathRegexStr, [RegexOptions]::Compiled -bor [RegexOptions]::IgnoreCase)

            $excludeFilePatternRegexStr = $this.CreateExcludedFilePatternRegexString($this.ExcludedFilePatterns)
            $excludeFilePatternRegex = [Regex]::new($excludeFilePatternRegexStr, [RegexOptions]::Compiled -bor [RegexOptions]::IgnoreCase)
    
            $trackedFilesToExclude = $trackedFilesToRefresh.Where({$file = $_; `
                ($this.ExcludedFolders -and $excludePathRegex.IsMatch($file.FullName)) -or `
                ($this.ExcludedFilePatterns -and $excludeFilePatternRegex.IsMatch($file.Name)) })  

            $itemsToUpdate.AddRange(($trackedFilesToExclude.ForEach({[PsCustomObject]@{ Operation='Remove'; File=$_ }})))
        }

        # Apply added, updated and removed files.
        $this.InternalApplyChanges($itemsToUpdate)

        $this.Paths = [List[string]] ($this.Paths.Where{ ([IO.DirectoryInfo]$_).Exists })

        $this.LastUpdated = Get-Date
    }

    hidden [IO.FileInfo[]] InternalGetFiles([IO.DirectoryInfo] $path)
    {
        Write-Progress -Activity "Adding or updating files" -Status "Collecting files: '$($path.FullName)'" `

        $getFilesArgs = @{

            Path = $path.FullName;
            File = $true;
            Recurse = $true;
            Force = $true;
            ErrorAction = "SilentlyContinue";
        }

        if ($this.ExcludedFilePatterns) { $getFilesArgs.Add("Exclude", $this.ExcludedFilePatterns) }
        if ($this.IncludedFilePatterns) { $getFilesArgs.Add("Include", $this.IncludedFilePatterns) }

        $applicableExcludedFolders = $this.ExcludedFolders | Where-Object { $_ -and $_.StartsWith(($path.FullName)) }
        
        $files = @(Get-ChildItem @getFilesArgs)

        if ($applicableExcludedFolders) {

            $regexStr = $this.CreateExcludedFilePathRegexString($applicableExcludedFolders)
            $excludedPathsRegex = [Regex]::new($regexStr, [RegexOptions]::Compiled -bor [RegexOptions]::IgnoreCase)

            $files = $files.Where({!$excludedPathsRegex.IsMatch($_.FullName)})
        }

        return $files 
    }

    hidden [PsCustomObject[]] InternalGetFilesToAddOrUpdate([IO.FileInfo[]] $files) 
    {
        $itemsToUpdate = [List[PsCustomObject]]@()
        $updateStatusTimer = [Diagnostics.Stopwatch]::StartNew()
        $durationTimer = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $files.Count; $i++) {

            $currentFile = $files[$i]

            if ($updateStatusTimer.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Adding or updating files" `
                    -Status "Analyzing differences..." `
                    -PercentComplete (($i / $files.Count) * 100) `
                    -CurrentOperation "($i of $($files.Count)) $($currentFile.FullName)" `
                    -SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($files.Count - $i)))

                $updateStatusTimer.Restart()
            }

            $trackedFile = $this.File.($currentFile.FullName)

            if ($trackedFile) {

                # Later modified files should be refreshed
                if ($trackedFile -lt $currentFile) {

                    $itemsToUpdate.Add(([PsCustomObject]@{ Operation='Remove'; File=$currentFile }))
                    $itemsToUpdate.Add(([PsCustomObject]@{ Operation='Add'; File=$currentFile }))
                }
            }
            else {
    
                # Newly added files
                $itemsToUpdate.Add(([PsCustomObject]@{ Operation='Add'; File=$currentFile }))
            }
        }

        return @($itemsToUpdate | Sort-Object -Property @{ Expression={$_.Operation}; Descending=$true; })
    }

    hidden InternalApplyChanges([PsCustomObject[]] $itemsToUpdate) 
    {
        $updateStatusTimer = [Diagnostics.Stopwatch]::StartNew()
        $saveProgressTimer =  [Diagnostics.Stopwatch]::StartNew()
        $durationTimer =  [Diagnostics.Stopwatch]::StartNew()


        for($i = 0; $i -lt $itemsToUpdate.Count; $i++ ) {
        
            $currentFile = $itemsToUpdate[$i].File
            $currentOperation = $itemsToUpdate[$i].Operation

            if ($updateStatusTimer.ElapsedMilliseconds -ge 500) 
            {
                $status = $currentOperation -eq "Add" ? "Calculating Hash..." : "Removing from FileHashTable..."	        

                Write-Progress -Activity "Adding or updating files" `
                    -Status $status `
                    -PercentComplete (($i / $itemsToUpdate.Count) * 100) `
                    -CurrentOperation "($i of $($itemsToUpdate.Count)) $($currentFile.FullName)" `
                    -SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($itemsToUpdate.Count - $i)))

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

    hidden [string] CreateExcludedFilePatternRegexString([string[]] $excludedPatterns) {

        $isExtensionRegex = [Regex]::new("^\*\.[a-z0-9_-]*$", [RegexOptions]::IgnoreCase)

        $myExcludedExtensions = $excludedPatterns.Where({ $isExtensionRegex.IsMatch($_) }).ForEach({ $_.Trim("*",".") })
        $myExcludedPatterns = $excludedPatterns.Where({ !$isExtensionRegex.IsMatch($_) }).ForEach({ "^$((($_ -replace "\.", "\.") -replace "\*", "(.*)"))" })
        
        return (@("(.*)\.($($myExcludedExtensions -Join "|"))$") + $myExcludedPatterns) -Join "|"
    }

    hidden [string] CreateExcludedFilePathRegexString([string[]] $excludedPaths) {
        
        return ($excludedPaths.ForEach({ "^$([regex]::Escape($_))(.*)" })) -Join "|"
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

        $fileItems = @($deserialized.File.GetEnumerator())
        $hashItems = @($deserialized.Hash.GetEnumerator())
        
        $sw = [Diagnostics.Stopwatch]::StartNew()
        $durationTimer = [Diagnostics.Stopwatch]::StartNew()

        for($i = 0; $i -lt $fileItems.Count; $i++ ) {
        
            $currentFileItem = $fileItems[$i]

            if ($sw.ElapsedMilliseconds -ge 500) 
            {
                Write-Progress `
                    -Activity "Importing FileHashTable..." `
                    -PercentComple ($i / $fileItems.Count * 100) `
                    -SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($fileItems.Count - $i)))

                $sw.Restart()
            }

            $newLookup.File.Add($currentFileItem.Name, [BasicFileInfo]$currentFileItem.Value)

            if ($i -lt $hashItems.Count) {

                $currentHashItem = $hashItems[$i]
                $newLookup.Hash.Add($currentHashItem.Name, ([BasicFileInfo[]]$currentHashItem.Value))
            }
        }

        $items = [string[]]@($deserialized.Paths | Where-Object { $_ }); if ($items) { $newLookup.Paths.AddRange($items) } 
        $items = [string[]]@($deserialized.IncludedFilePatterns | Where-Object { $_ }); if ($items) { $newLookup.IncludedFilePatterns.AddRange($items) } 
        $items = [string[]]@($deserialized.ExcludedFilePatterns | Where-Object { $_ }); if ($items) { $newLookup.ExcludedFilePatterns.AddRange($items) } 
        $items = [string[]]@($deserialized.ExcludedFolders | Where-Object { $_ }); if ($items) { $newLookup.ExcludedFolders.AddRange($items) } 

        $newLookup.LastUpdated = $deserialized.LastUpdated
        $newLookup.SavedAsFile = $deserialized.SavedAsFile

        return $newLookup
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
        $durationTimer = [Diagnostics.Stopwatch]::StartNew()
        
        $otherFiles = @($other.File.Values.GetEnumerator()) 
        
        for($i = 0; $i -lt $otherFiles.Count; $i++) {
            
            $currentFile = $otherFiles[$i]
            
            if ($sw.Elapsed.TotalMilliseconds -ge 500) 
            {
                Write-Progress -Activity "Comparing hashes" `
                    -Status "($i of $($otherFiles.Count)) Processing $($currentFile.FullName)" `
                    -PercentComplete ($i / $otherFiles.Count * 100) `
                    -SecondsRemaining ($durationTimer.Elapsed.TotalSeconds / (([Math]::Max($i, 1)) * ($otherFiles.Count - $i)))

                $sw.Restart()
            }

            if ($this.Hash.ContainsKey($currentFile.Hash))  
            {
                 $matchesLookup.Add($currentFile)
            }
            else 
            {
                $differencesLookup.Add($currentFile)
            }
        }

        return [PsCustomObject]@{ Matches=$matchesLookup; Differences=$differencesLookup; }
    }

    [string] ToString() 
    {
        $msg = [Text.StringBuilder]::new("`nFileHashTable is empty.`n")
            
        if ($this.File.Keys.Count -gt 0) {

            $size = $this.File.Values | Measure-Object -Prop Length -Sum | Select-Object -exp Sum
            $sizeMsg = if (($size / 1Gb) -ge 1) { "$([Math]::Round($size / 1Gb, 2)) Gb" } else { "$([Math]::Round($size / 1Mb, 2)) Mb" }

            $msg = [Text.StringBuilder]::new("`nFileHashTable contains $($this.File.Keys.Count) files ($sizeMsg).`n")
        }
            
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