using module '.\FileHashLookup.Impl.psm1'

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