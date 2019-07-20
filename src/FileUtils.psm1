function Copy-KeepExisting {

    [cmdletbinding(SupportsShouldProcess)]
    param(  
        [Parameter(Mandatory,ValueFromPipeline,ValueFromPipelineByPropertyName)]
        [Alias('FullName')]
        [IO.FileInfo]$file,
        [Parameter(Position=1)]
        [IO.DirectoryInfo] $destinationPath
    ) 

    BEGIN {

        if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
        if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }
    }

    PROCESS {

        $destinationPath = [IO.DirectoryInfo](GetAbsolutePath $destinationPath)
       
        $fileParentFolders = (($file.Directory.FullName -replace [regex]::Escape($file.Directory.Root)) -split [regex]::Escape([IO.Path]::DirectorySeparatorChar))

        $firstMatchingFolder = $fileParentFolders | ?{ Test-Path (Join-Path $destinationPath $_) } | Select-Object -First 1 

        if ($destinationPath.FullName -eq $file.Directory.FullName) 
        {
            $destinationFolder = $destinationPath         
        }
        elseif ($firstMatchingFolder) 
        {
            $destinationFolder = Join-Path $destinationPath (($fileParentFolders | Select-Object -Skip ($fileParentFolders.IndexOf($firstMatchingFolder))) -join [IO.Path]::DirectorySeparatorChar) 
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

    BEGIN {

        if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
        if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }
    }

    PROCESS {

        $destinationPath = [IO.DirectoryInfo](GetAbsolutePath $destinationPath)

        $destinationFolder = if ($directlyIntoTargetFolder) { $destinationPath } else { (Join-Path $destinationPath $folder.Name)}
        $files = Get-ChildItem $folder.FullName -file -recurse -force 

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

    BEGIN {

        if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
        if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }
    }

    PROCESS {

        $folder | Copy-FolderKeepExisting $destinationPath -directlyIntoTargetFolder:$directlyIntoTargetFolder
    
        if ((Test-Path $folder.FullName)) {
    
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
    BEGIN {

        if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
        if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }
    }

    PROCESS {

        $destinationPath = [IO.DirectoryInfo](GetAbsolutePath $destinationPath)

        $file | Copy-KeepExisting $destinationPath

        if ((Test-Path $file.FullName)) {
    
            $file | Remove-Item -Recurse -Force -ErrorAction Continue -Verbose:($verbosePreference -eq 'Continue')
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

    if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
    if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }

    $destinationFile = [IO.FileInfo](Join-Path $destinationPath $file.Name)

    if ($pscmdlet.ShouldProcess("`n    $($file.FullName) `n    $($destinationFile.FullName)", "Copy-KeepExisting")) {
        
        if (!(Test-Path $destinationFile.Directory)) {
            
            New-Item -ItemType Directory $destinationFile.Directory -Force -Verbose:$false > $null
        }
    
        if (!(Test-Path $destinationFile)) {
            
            Copy-Item -Path $file.FullName -Destination $destinationFile -Force -Verbose:($verbosePreference -eq 'Continue')
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
        
            Copy-Item -Path $file.FullName -Destination $destinationFile -Force -Verbose:($verbosePreference -eq 'Continue')
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

    BEGIN {

        if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }
        if (!$PSBoundParameters.ContainsKey('WhatIf')) { $WhatIfPreference = $PSCmdlet.GetVariableValue('WhatIfPreference') }
    }

    PROCESS {

        $path = [IO.DirectoryInfo](GetAbsolutePath $path)

        $folders = Get-ChildItem $path.FullName -Recurse -Directory -Force | Sort-Object -Property @{ Expression={ $_.FullName.Length }; Ascending=$true } 
        $emptyFolders = $folders | Where-Object { $null -eq (Get-ChildItem $_.FullName -file -Recurse -Force) } 

        $emptyFolders | ForEach-Object { if (Test-Path $_.FullName -ErrorAction Continue) { Remove-Item $_.FullName -Force -Recurse } }
    }
}

function GetAbsolutePath {
    param([string] $path)

    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
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