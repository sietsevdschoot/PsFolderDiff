using module '.\FileHashLookup.Impl.psm1'
using module '.\BasicFileInfo.psm1'
using namespace System.Collections.Generic

<#
    .SYNOPSIS
    Allows finding duplicate files
    
    .DESCRIPTION
    Finds duplicate entries by FileHash, Optionally pass a SortOrder for files to keep, the default sort order is by FileName
    Returns a structure per duplicate, containing the file to keep, and a list of duplicates of that entry.
    
    .PARAMETER FileHashLookup
    The FileHashLookup to check for duplicates
    
    .PARAMETER SortExpression
    ScriptBlock containing a sort expression, given an array of files as argument.
    
    .EXAMPLE

    $duplicates = Get-Duplicates $myFileHashTable -SortExpression { param([IO.FileInfo[]] $files) $files | Sort-Object @{ Expression={$_.Directory.Name.Length }; Ascending=$true }  }

    .EXAMPLE
    Second Example
#>
Function Get-Duplicates {
  [CmdletBinding(DefaultParameterSetName="SortExpression")]
  param(
    [Parameter(Mandatory,ValueFromPipeline, Position=0)]
    [FileHashLookup] $FileHashLookup,
    [Alias("SortBy")]
    [Parameter(ParameterSetName="ScriptBlock")]
    [ScriptBlock] $SortScriptBlock,
    [Parameter(ParameterSetName="SortExpression")]
    [PsCustomObject] $SortExpression
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
  
        $progressArgs.('Status') = "[2 / 2] Sorting duplicates and selecting items to keep. ($i of $($duplicateHashEntries.Count)) $($entry[0].FullName)"
        $progressArgs.('PercentComplete') = $i / $duplicateHashEntries.Count * 100 
        Write-Progress @progressArgs
        $sw.Restart()
      }
  
      if ($SortScriptBlock) {

        $files = ($SortScriptBlock.Invoke((,$entry.Value)) | ForEach-Object{ [BasicFileInfo]$_ })

      }
      elseif ($SortExpression) {

        $files = $entry.Value | Sort-Object -Property $SortExpression

      }
      else {
      
        $files = @($entry.Value | Sort-Object -prop FullName)  
      }

      $newEntry = [PsCustomObject] @{ 
        Keep = ($files | Select-Object -First 1);
        Duplicates = @($files | Select-Object -Skip 1);
      }

      $foundDuplicates.Add($newEntry)
    }
  }

  END {
    
    Write-Progress @progressArgs -Completed
    $foundDuplicates
  }
}

Export-ModuleMember -Function Get-Duplicates