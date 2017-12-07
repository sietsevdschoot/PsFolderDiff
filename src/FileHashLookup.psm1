#using module .\FileHashLookup.Impl.psm1

$script = [ScriptBlock]::Create(((cat "$PSScriptRoot\FileHashLookup.Impl.psm1") -join "`r`n"))
$module = New-Module 'FileHashLookup-Module' $script | Import-Module -PassThru

function Import-FileHashTable {
    [CmdletBinding()]
    param (
        [IO.FileInfo] $file
    )

    if ($module) {
        & $module.NewBoundScriptBlock({[FileHashLookup]::Load($args[0])}) $file.FullName
    }
    else {
        [FileHashLookup]::Load($file.FullName)    
    }
}

function Get-FileHashTable {
    [CmdletBinding()]
    param (
        [IO.DirectoryInfo] $path = $null
    )

    if ($module) {
        if ($path) {
            & $module.NewBoundScriptBlock({[FileHashLookup]::New($args[0])}) $path
        }
        else {
            & $module.NewBoundScriptBlock({[FileHashLookup]::New()})
        }
    }
    else {
        if ($path) {
            [FileHashLookup]::New($path)
        }
        else {
            [FileHashLookup]::New()
        }
    }
}

Set-Alias GetFileHashTable Get-FileHashTable
Set-Alias Get-HashTable Get-FileHashTable

Set-Alias ImportFileHashTable Import-FileHashTable
Set-Alias Import-HashTable Import-FileHashTable

Export-ModuleMember -Function Get-FileHashTable -Alias @("GetFileHashTable","Get-HashTable")
Export-ModuleMember -Function Import-FileHashTable -Alias @("ImportFileHashTable","Import-HashTable")