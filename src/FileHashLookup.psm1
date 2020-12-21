using module .\FileHashLookup.Impl.psm1

<#
    .SYNOPSIS
    Allows for comparison of folder contents.
    .DESCRIPTION
    Builds a [FileHashLookup] of the contents of a directory. 
    generates a two-way hashs table of the contents of a directory
    .PARAMETER path
    Path to folder to build a file hash table of.
    .EXAMPLE
    First Example
    .EXAMPLE
    Second Example
#>
function Get-FileHashTable {
    [CmdletBinding()]
    param (
        [IO.DirectoryInfo] $path = $null
    )

    if ($path) {
        [FileHashLookup]::New($path)
    }
    else {
        [FileHashLookup]::New()
    }
}

<#
    .SYNOPSIS
    Loads a [FileHashTable] from a file
    .DESCRIPTION
    Loads a [FileHashTable] from a file
    .PARAMETER path
    Path to previously saved .xml file.
    .EXAMPLE
    First Example
    .EXAMPLE
    Second Example
#>
function Import-FileHashTable {
    [CmdletBinding()]
    param (
        [IO.FileInfo] $file
    )

    [FileHashLookup]::Load($file)    
}

Set-Alias GetFileHashTable Get-FileHashTable
Set-Alias Get-HashTable Get-FileHashTable

Set-Alias ImportFileHashTable Import-FileHashTable
Set-Alias Import-HashTable Import-FileHashTable

Export-ModuleMember -Function Get-FileHashTable -Alias @("GetFileHashTable","Get-HashTable")
Export-ModuleMember -Function Import-FileHashTable -Alias @("ImportFileHashTable","Import-HashTable")