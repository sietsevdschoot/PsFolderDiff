using module '.\FileHashLookup.Impl.psm1'
using namespace System.Collections.Generic

#1. Select potential duplicates. Input [FileHashLookup] Output [IO.FileInfo][][]
#2. Sort potential duplicates (A-Z. 1st is SelectedItem). Input [IO.FileInfo][] Output [PsCustomObject][]
#3. Filter potential duplicates. Input [PsCustomObject][] Output [bool]
#4. Convert back to FileInfo. Input [PsCustomObject] Output [IO.FileInfo]
#5. Convert to OutputFormat. Input [IO.FileInfo][] Output @{ Selected=[IO.FileInfo]; Duplicates=[IO.FileInfo][]; All=[IO.FileInfo][]; }
Function Get-DuplicatesInternal {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory,ValueFromPipeline, Position=0)]
    [FileHashLookup] $fileHashLookup,
    [Parameter(Position=1)]
    [ScriptBlock] $getPotentialDuplicates,
    [ScriptBlock] $sortPotentialDuplicatesIntermediate,
    [ScriptBlock] $filterPotentialDuplicatesIntermediate = { param([PsCustomObject]$entry) $true },
    [ScriptBlock] $convertIntermediateToFileInfo = { param([PsCustomObject]$entry) [IO.FileInfo]$entry.FullName }
  ) 

  BEGIN {

    if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
  }

  PROCESS {

    $sw = [Diagnostics.Stopwatch]::StartNew()

    $progressArgs = @{
      Activity = "Find duplicates.";
      Status = "[1 / 5] Selecting potential duplicates"
    }
  
    Write-Progress @progressArgs
  
    #1. Select potential duplicates. Input [FileHashLookup] Output [IO.FileInfo][][]
    $potentialDuplicateGroups = getPotentialDuplicates.Invoke($fileHashLookup)
  
    #2. Sort potential duplicates. Input [IO.FileInfo][] Output [PsCustomObject][]
    $sortedEntryGroups = [List[PsCustomObject][]]@{}
    
    for($i = 0; $i -lt $potentialDuplicateGroups.Count; $i++) {
    
      $currentEntryGroup = $potentialDuplicateGroups[$i]
      
      if ($sw.ElapsedMilliseconds -ge 500) {
  
        $progressArgs.('Status') = "[2 / 5] Sorting potential duplicates. ($i of $($potentialDuplicateGroups.Count)) $($currentEntryGroup[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $potentialDuplicateGroups.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      $sortedEntryGroup = $sortPotentialDuplicatesIntermediate.Invoke($currentEntryGroup)
      $sortedEntryGroups.Add($sortedEntryGroup)
    }
  
    #3. Filter potential duplicates. Input [PsCustomObject][] Output [bool]
    $filteredEntryGroups = [List[PsCustomObject][]]@{}
    
    for($i = 0; $i -lt $sortedEntryGroups.Count; $i++) {
    
      $currentEntryGroup = $sortedEntryGroups[$i]
      
      if ($sw.ElapsedMilliseconds -ge 500) {
  
        $progressArgs.('Status') = "[3 / 5] Filter potential duplicates. ($i of $($sortedEntryGroups.Count)) $($currentEntryGroup[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $sortedEntryGroups.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      if ($filterPotentialDuplicatesIntermediate.Invoke($currentEntryGroup)) {
  
        $filteredEntryGroups.Add($currentEntryGroup) 
      }
    }
  
    #4. Convert back to FileInfo. Input [PsCustomObject] Output [IO.FileInfo]
    $convertedEntryGroups = [List[IO.FileInfo][]]@{}
    
    for($i = 0; $i -lt $filteredEntryGroups.Count; $i++) {
    
      $currentEntryGroup = $filteredEntryGroups[$i]
      
      if ($sw.ElapsedMilliseconds -ge 500) {
  
        $progressArgs.('Status') = "[4 / 5] Converting back to FileInfo. ($i of $($filteredEntryGroups.Count)) $($currentEntryGroup[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $filteredEntryGroups.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      $convertedEntryGroup = $currentEntryGroup | ForEach-Object { $convertIntermediateToFileInfo.Invoke($_) }
      $convertedEntryGroups.Add($convertedEntryGroup)
    }
  
    #5. Convert to OutputFormat. Input [IO.FileInfo][] Output @{ Selected=[IO.FileInfo]; Duplicates=[IO.FileInfo][]; All=[IO.FileInfo][]; }  
    $outputEntries = [List[PsCustomObject]]@{}
    
    for($i = 0; $i -lt $convertedEntryGroups.Count; $i++) {
    
      $currentEntryGroup = $convertedEntryGroups[$i]
      
      if ($sw.ElapsedMilliseconds -ge 500) {
  
        $progressArgs.('Status') = "[5 / 5] Create output entries. ($i of $($convertedEntryGroups.Count)) $($currentEntryGroup[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $convertedEntryGroups.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      $outputEntry = $currentEntryGroup | Select-Object `
        @{ Name="Selected"; Expression={$_ | Select-Object -First 1}},
        @{ Name="Duplicates"; Expression={$_ | Select-Object -Skip 1}}, 
        @{ Name="All"; Expression={$_}}
  
      $outputEntries.Add($outputEntry)
    }    
  }

  END {

    $outputEntries
  }
}


Function Get-FoldersContainingDuplicates {
  param (
    [FileHashLookup] $fileHashLookup  
  )
  
  $filename = [IO.FileInfo]$fileHashLookup.SavedAsFile 

  $folders = Get-DuplicateFolders $fileHashLookup

  $foldersByNrOfFiles = $folders `
    | ForEach-Object { [PSCustomObject] @{ Path=$_.FullName; NrOfFiles=(Get-ChildItem $_.FullName -File -Recurse -Force).Length } } `
    | Sort-Object -Property NrOfFiles -Descending
      
  $foldersByNrOfFiles 
}

Function Get-DuplicateFolders {
  param (
    [FileHashLookup] $fileHashLookup  
  )
  
  $paths = @($fileHashLookup.Paths)

  $folders = $fileHashLookup.GetDuplicates().GetFiles() | Select-Object -exp Directory | Select-Object -Unique
  $allFolders = [Collections.ArrayList]$folders

  foreach($folder in $folders) {
  
    $parent = $folder

    do {

      $parent = $parent.Parent

      $allFolders.Add($parent) > $null
    }
    while (($null -eq ($paths | Where-Object{ $_ -eq $parent.FullName })) -and ($null -ne $parent)) 
  }

  $allFolders | Select-Object -Unique | Sort-Object -prop @{ Expression={$_.FullName.Split([IO.Path]::DirectorySeparatorChar)} }
}

Function ShowDuplicates {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [FileHashLookup] $fileHashLookup
    )

    $duplicates = $fileHashLookup.Hash.GetEnumerator() | Where-Object { @($_.Value).Count -gt 1 } 
    
    if ($duplicates) {
    
        $duplicates | Foreach-Object { "$($_.Name) `n$((@($_.Value) | Foreach-Object { "`t > $(([IO.FileInfo]$_).Fullname)" }) -join "`n")" }
    }
    else {
        
        "No duplicates found"
    }
}

Function Get-DuplicatesByName {
  [CmdletBinding()]
  param(
    [Parameter(ValueFromPipeline)]
    [FileHashLookup] $fileHashLookup
  )

  $duplicates = @{}

  $groupByName = $fileHashLookup.GetFiles() | Group-Object -prop Name 

  $groupByName.GetEnumerator() | Where-Object { @($_.Group).Count -gt 1} | ForEach-Object { $duplicates.Add($_.Name, @($_.Group)) }

  $duplicates 
}

Set-Alias displayDuplicates ShowDuplicates
Export-ModuleMember -Function ShowDuplicates -Alias @("displayDuplicates")

Export-ModuleMember -Function Get-FoldersContainingDuplicates
Export-ModuleMember -Function Get-DuplicatesByName