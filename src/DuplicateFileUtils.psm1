using module '.\FileHashLookup.Impl.psm1'
using module '.\BasicFileInfo.psm1'
using namespace System.Collections.Generic

Function Get-Duplicates {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory,ValueFromPipeline, Position=0)]
    [FileHashLookup] $fileHashLookup,
    [Alias("SortBy")]
    [Parameter(Mandatory=$false)]
    [ScriptBlock] $SortExpression
  ) 

  BEGIN {

    if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
  }

  PROCESS {

    $duplicateHashEntries = $fileHashLookup.Hash.GetEnumerator() | Where-Object{ @($_.Value).Count -gt 1 } 

    foreach ($entry in $duplicateHashEntries) {

      $files = $SortExpression `
        ? ($SortExpression.Invoke((,@($entry.Value | ForEach-Object {[IO.FileInfo]$_}))) | ForEach-Object{ [BasicFileInfo] $_ })
        : @($entry.Value | Sort-Object -prop FullName)
      
      [PsCustomObject] @{ 
          Keep=($files | Select-Object -First 1);
          Duplicates=(,@($files | Select-Object -Skip 1));
      }
    }
  }

  END {

  }
}


Function Get-DuplicatesInternal {
  [CmdletBinding()]
  param(
    [Parameter(Mandatory,ValueFromPipeline, Position=0)]
    [FileHashLookup] $fileHashLookup
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

Export-ModuleMember -Function Get-Duplicates