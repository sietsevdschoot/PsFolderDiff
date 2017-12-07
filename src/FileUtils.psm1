function Copy-KeepExisting {

    [cmdletbinding(SupportsShouldProcess)]
    param(  
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.FileInfo]$file,
        [Parameter(Position=1)]
        [IO.DirectoryInfo] $destinationPath
    ) 

    PROCESS {

        if (!$destinationPath.Exists) {

            $destinationPath = [IO.DirectoryInfo](Join-Path (Get-Location) $destinationPath.Name)
        }
       
        $fileParentFolders = (($file.Directory.FullName -replace [regex]::Escape($file.Directory.Root)) -split [regex]::Escape([IO.Path]::DirectorySeparatorChar))

        $firstMatchingFolder = $fileParentFolders | ?{ Test-Path (Join-Path $destinationPath $_) } | Select -First 1 

        if ($destinationPath.FullName -eq $file.Directory.FullName) 
        {
            $destinationFolder = $destinationPath         
        }
        elseif ($firstMatchingFolder) 
        {
            $destinationFolder = Join-Path $destinationPath (($fileParentFolders | Select -Skip ($fileParentFolders.IndexOf($firstMatchingFolder))) -join [IO.Path]::DirectorySeparatorChar) 
        } 
        else 
        {
            $destinationFolder = Join-Path $destinationPath ($file.Directory.FullName -replace [regex]::Escape($file.Directory.Root)) 
        }

        Internal-Copy-KeepExisting -file $file -destinationPath $destinationFolder
    }
}


function Copy-FolderKeepExisting {

    [cmdletbinding(SupportsShouldProcess)]
    param(  
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.DirectoryInfo] $folder,
        [Parameter(Position=1)]
        [IO.DirectoryInfo] $destinationPath,
        [switch] $directlyIntoTargetFolder
    ) 

    PROCESS {

        if (!$destinationPath.Exists) {

            $destinationPath = [IO.DirectoryInfo](Join-Path (Get-Location) $destinationPath.Name)
        }

        $destinationFolder = if ($directlyIntoTargetFolder) { $destinationPath } else { (Join-Path $destinationPath $folder.Name)}
        $files = dir $folder.FullName -file -recurse -force 

        foreach ($file in $files) {
        
            $fileDestinationFolder = (Join-Path $destinationFolder ($file.Directory.FullName -replace [regex]::Escape($folder.FullName)).TrimStart([IO.Path]::DirectorySeparatorChar))

            Internal-Copy-KeepExisting -file $file -destinationPath $fileDestinationFolder
        }
    }
}

function Move-FolderKeepExisting { 

   [cmdletbinding(SupportsShouldProcess)]
    param(  
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.DirectoryInfo] $folder,
        [Parameter(Position=1)]
        [IO.DirectoryInfo] $destinationPath,
        [switch] $directlyIntoTargetFolder
    ) 

    PROCESS {

        $folder | Copy-FolderKeepExisting $destinationPath -directlyIntoTargetFolder:$directlyIntoTargetFolder
    
        if (Test-Path $folder) {
    
            $folder | del -Recurse -Force -ErrorAction Continue
        }
    }
}

function Move-KeepExisting {

    [cmdletbinding(SupportsShouldProcess)]
    param(  
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.FileInfo]$file,
        [Parameter(Position=1)]
        [IO.DirectoryInfo] $destinationPath
    ) 

    PROCESS {

        if (!$destinationPath.Exists) {
        
            $destinationPath = [IO.DirectoryInfo](Join-Path (Get-Location) $destinationPath.Name)
        }

        $file | Copy-KeepExisting $destinationPath

        if (Test-Path $file) {
    
            $file | del -Recurse -Force -ErrorAction Continue
        }
    }
}

Function Internal-Copy-KeepExisting {
    [cmdletbinding(SupportsShouldProcess)] 
    param(
        [Parameter(Position=0, Mandatory)]
        [IO.FileInfo]$file,
        [Parameter(Position=1, Mandatory)]
        [IO.DirectoryInfo] $destinationPath
    )    

    $destinationFile = [IO.FileInfo](Join-Path $destinationPath $file.Name)

    if ($pscmdlet.ShouldProcess("`n    $($file.FullName) `n    $($destinationFile.FullName)", "Copy-KeepExisting")) {
        
        if (!(Test-Path $destinationFile.Directory)) {
            
            New-Item -ItemType Directory $destinationFile.Directory -Force -Verbose:$false > $null
        }
    
        if (!(Test-Path $destinationFile)) {
            
            Copy-Item -Path $file.FullName -Destination $destinationFile -Force
        }
        else
        {
            $i = 1

            do 
            {
                if ($i -eq 1) {
            
                    $destinationFile = ("{0} - Copy{1}" -f (Join-Path $destinationPath ($file.Name -replace $file.Extension)), $file.Extension)

                } else {
                
                    $destinationFile = ("{0} - Copy ({1}){2}" -f (Join-Path $destinationPath ($file.Name -replace $file.Extension)), $i, $file.Extension)
                }
            
                $i += 1
            }
            while (Test-Path $destinationFile) 
        
            Copy-Item -Path $file.FullName -Destination $destinationFile -Force
        }
    }
}

Function Remove-EmptyFolders {
    [cmdletbinding(SupportsShouldProcess)] 
    param (
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.DirectoryInfo] $path
    )

    PROCESS {

        if (!$path.Exists) {
    
            $path = [IO.DirectoryInfo](Join-Path (Get-Location) $path.Name)
        }

        $folders = dir $path.FullName -Recurse -Directory -Force | Sort -Property @{ Expression={ $_.FullName.Length }; Ascending=$true } 
        $emptyFolders = $folders | ?{ (dir $_.FullName -file -Recurse -Force) -eq $null } 

        $emptyFolders | %{ if (Test-Path $_.FullName -ErrorAction Continue) { del $_.FullName -Force -Recurse } }
    }
}

Set-Alias CopyKeepExisting Copy-KeepExisting
Set-Alias MoveKeepExisting Move-KeepExisting
Set-Alias CopyFolderKeepExisting Copy-FolderKeepExisting 
Set-Alias MoveFolderKeepExisting Move-FolderKeepExisting 
Set-Alias RemoveEmptyFolders Remove-EmptyFolders

Export-ModuleMember Copy-KeepExisting -Alias @("CopyKeepExisting")
Export-ModuleMember Move-KeepExisting -Alias @("MoveKeepExisting")
Export-ModuleMember Copy-FolderKeepExisting -Alias @("CopyFolderKeepExisting")
Export-ModuleMember Move-FolderKeepExisting -Alias @("MoveFolderKeepExisting")
Export-ModuleMember Remove-EmptyFolders -Alias @("RemoveEmptyFolders")