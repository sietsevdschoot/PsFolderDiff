using module .\FileHashLookup.Impl.psm1

function Import-FileHashTable {
    
    param (
        [IO.FileInfo] $file
    )

    [FileHashLookup]::Load($file.FullName)

}

function Get-FileHashTable {

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

Set-Alias GetFileHashTable Get-FileHashTable
Set-Alias Get-HashTable Get-FileHashTable

Set-Alias ImportFileHashTable Import-FileHashTable
Set-Alias Import-HashTable Import-FileHashTable

Export-ModuleMember -Function Get-FileHashTable -Alias @("GetFileHashTable","Get-HashTable")
Export-ModuleMember -Function Import-FileHashTable -Alias @("ImportFileHashTable","Import-HashTable")