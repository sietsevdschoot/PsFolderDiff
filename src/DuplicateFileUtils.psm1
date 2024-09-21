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
    
    $sw = [Diagnostics.Stopwatch]::StartNew()

    $foundDuplicates = [List[PsCustomObject]]@()
  }

  PROCESS {
    
    $progressArgs = @{
      Activity = "Find duplicates.";
      Status = "[1 / 2] Selecting Duplicates"
    }
  
    Write-Progress @progressArgs

    $duplicateHashEntries = $fileHashLookup.Hash.GetEnumerator() | Where-Object{ @($_.Value).Count -gt 1 } 

    for($i = 0; $i -lt $duplicateHashEntries.Count; $i++) {
    
      $entry = $duplicateHashEntries[$i]

      if ($sw.ElapsedMilliseconds -ge 500) {
  
        $progressArgs.('Status') = "[2 / 2] Sorting duplicates and selecting items to keep. ($i of $($potentialDuplicateGroups.Count)) $($currentEntryGroup[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $duplicateHashEntries.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      $files = $SortExpression `
        ? ($SortExpression.Invoke((,@($entry.Value | ForEach-Object {[IO.FileInfo]$_}))) | ForEach-Object{ [BasicFileInfo] $_ })
        : @($entry.Value | Sort-Object -prop FullName)
      
      $foundDuplicates.Add([PsCustomObject] @{ 
        Keep = ($files | Select-Object -First 1);
        Duplicates = (,@($files | Select-Object -Skip 1));
      })
    }
  }

  END {
    $foundDuplicates
  }
}

Export-ModuleMember -Function Get-Duplicates