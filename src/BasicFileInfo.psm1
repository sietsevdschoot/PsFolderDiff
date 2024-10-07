using namespace System.Collections.Generic

class BasicFileInfo : IComparable
{
    BasicFileInfo()
    {
      $this.Init($null, $null)
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
        $this | Add-Member -Name Hash -MemberType ScriptProperty `
            -Value { 
          
                if ([string]::IsNullOrEmpty($this._hash)) {

                    $this._hash = Get-FileHash -LiteralPath $this.FullName -Algorithm MD5 -ErrorAction SilentlyContinue | Select-Object -exp Hash
                }
                
                return $this._hash 
            } `
            -SecondValue { param($value) $this._hash = $value }

        $this.Hash = $hash

        if ($file) {
            $this.FullName = $file.FullName
            $this.CreationTime = $file.CreationTime
            $this.LastWriteTime = $file.LastWriteTime
            $this.Length = $file.Length
        }

        $this._validTypes = (@([IO.FileInfo], [BasicFileInfo]) | Select-Object -exp Name )
    }

    [string] $FullName
    [DateTime] $CreationTime
    [DateTime] $LastWriteTime
    [long] $Length

    hidden [string[]] $_validTypes
    hidden [string] $_hash

    [bool] Equals ($that) 
    {
        if (!$this._validTypes.Contains($that.GetType().Name)) {
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
        if (!$this._validTypes.Contains($that.GetType().Name)) {
            Throw "this: '$(($this | ConvertTo-Json))' That [$($that.GetType().Name)]: $(($that | ConvertTo-Json)). Can't compare"
        }

        $diff = (($this.CreationTime - $that.CreationTime) + ($this.LastWriteTime - $that.LastWriteTime)).TotalMilliseconds  
        
        $compare = ($diff -eq 0) ? 0 : ($diff -gt 1) ? 1 : -1

        return $compare
    }    

    static [IO.FileInfo] op_Implicit([BasicFileInfo] $instance) {
        
        return [IO.FileInfo] $instance.FullName
    }

    hidden static [BasicFileInfo] op_Implicit([string] $fullName){
  
      return [BasicFileInfo](Get-Item $fullName)
    } 
}
