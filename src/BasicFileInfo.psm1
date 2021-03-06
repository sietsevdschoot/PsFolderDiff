using namespace System.Collections.Generic

class BasicFileInfo : IComparable
{
    BasicFileInfo()
    {
    }
    
    BasicFileInfo([IO.FileInfo] $file)
    {
        $this.Init($file, $null)
    }

    BasicFileInfo([IO.FileInfo] $file, [string] $hash)
    {
        $this.Init($file, $hash)
    }

    hidden [void] Init([IO.FileInfo] $file, [string] $hash)
    {
        $this.FullName = $file.FullName
        $this.CreationTime = $file.CreationTime
        $this.LastWriteTime = $file.LastWriteTime
        $this.Length = $file.Length

        # Null coalescing doesn't work here because passing $null as a string becomes "" for $hash :(
        $this.Hash = ![string]::IsNullOrEmpty($hash) `
            ? $hash `
            : (Get-FileHash -LiteralPath $file -Algorithm MD5 -ErrorAction SilentlyContinue).Hash

        $this.ValidTypes = [HashSet[string]](@([IO.FileInfo], [BasicFileInfo]) | ForEach-Object { $_.Name })
    }

    [string] $FullName
    [DateTime] $CreationTime
    [DateTime] $LastWriteTime
    [string] $Hash
    [long] $Length

    hidden [HashSet[string]] $ValidTypes

    [bool] Equals ($that) 
    {
        if (!$this.ValidTypes.Contains($that.GetType().Name)) {
            return $false
        }

        $isEqual = $this.FullName -eq $that.FullName `
            -and $this.CreationTime -eq $that.CreationTime `
            -and $this.LastWriteTime -eq $that.LastWriteTime `
            -and $this.Length -eq $that.Length

        return $that -is [IO.FileInfo] `
            ? $isEqual `
            : $isEqual -and $this.Hash -eq $that.Hash 
    }    

    [int] CompareTo($that)
    {
        if (!$this.ValidTypes.Contains($that.GetType())) {
            Throw "Can't compare"
        }
      
        return (($this.CreationTime - $that.CreationTime) + ($this.LastWriteTime - $that.LastWriteTime)).TotalMilliseconds
    }    

    static [IO.FileInfo] op_Implicit([BasicFileInfo] $instance) {
        
        return [IO.FileInfo] $instance.FullName
    }
}
