using module '.\FileHashLookup.Impl.psm1'

Function Get-FoldersContainingDuplicates {
  param (
    [FileHashLookup] $fileHashLookup  
  )
  
  $filename = [IO.FileInfo]$fileHashLookup.SavedAsFile 

  $folders = Get-DuplicateFolders $fileHashLookup

  $foldersByNrOfFiles = $folders | %{ [PSCustomObject]@{ Path=$_.FullName; NrOfFiles=(dir $_.FullName -File -Recurse -Force).Length } } | Sort -Property NrOfFiles -Descending
   
  $foldersByNrOfFiles | Select -exp Path | Out-File ($filename.FullName -replace $filename.Extension, "_folderorder.txt")
      
  $foldersByNrOfFiles | ft NrOfFiles, Path -AutoSize | Out-String | %{ Write-Host $_ }

  $foldersByNrOfFiles 
}

Function Get-DuplicateFolders {
  param (
    [FileHashLookup] $fileHashLookup  
  )
  
  $paths = @($fileHashLookup.Paths)

  $folders = $fileHashLookup.GetDuplicateFiles() | Select -exp Directory | Select -Unique
  $allFolders = [Collections.ArrayList]$folders

  foreach($folder in $folders) {
  
    $parent = $folder

    do {

      $parent = $parent.Parent

      $allFolders.Add($parent) > $null
    }
    while ((($paths | ?{ $_ -eq $parent.FullName }) -eq $null) -and ($parent -ne $null)) 
  }

  $allFolders | Select -Unique
}
