using namespace System.Collections.Generic

Describe "BasicFileInfo" {

    BeforeAll {

        & ".\Reload.ps1"

        $originalLocation = Get-Location 
    }

    AfterAll {

        Set-Location $originalLocation

        & ".\Reload.ps1" -unload
    }
    
    BeforeEach {
        
        1..3 | ForEach-Object { New-Item -ItemType File Testdrive:\MyFolder\$_.txt -Value "My Test Value $_" -Force  }
        Set-Location $TestDrive
    }

    AfterEach {
        
        Get-ChildItem $TestDrive -Directory -Recurse | Remove-Item -Force -Recurse
        Get-ChildItem $TestDrive -file -Recurse | Remove-Item -Force
    }    

    It "Compare same file as FileInfo and BasicFileInfo equals" {
        
        $file =  [IO.FileInfo]"$Testdrive\MyFolder\1.txt"
        $basicFileInfo = [BasicFileInfo]::new($file)

        $basicFileInfo -eq $file | Should -BeTrue
    }
        
    It "Compare same file as BasicFileInfo equals" {
        
        $file =  [IO.FileInfo]"$Testdrive\MyFolder\1.txt"
        $basicFileInfo1 = [BasicFileInfo]::new($file)
        $basicFileInfo2 = [BasicFileInfo]::new($file)

        $basicFileInfo1 -eq $basicFileInfo2 | Should -BeTrue
    }

    It "Later modified file is greater than original file, due LastWriteTime." {
        
        $file =  [IO.FileInfo]"$Testdrive\MyFolder\1.txt"
        $basicFileInfo = [BasicFileInfo]::new($file)

        Set-Content $file -Value "Updated!"

        $updatedFile =  [IO.FileInfo]"$Testdrive\MyFolder\1.txt"

        $basicFileInfo | Should -BeLessThan $updatedFile 
    }
        
    It "Can be saved and loaded again" {
        
        $file =  [IO.FileInfo]"$Testdrive\MyFolder\1.txt"
        $basicFileInfo = [BasicFileInfo]::new($file)

        $tempFile = New-TemporaryFile

        $basicFileInfo | Export-Clixml -Path $tempFile.FullName
        $loadedBasicFileInfo = [BasicFileInfo](Import-Clixml -Path $tempFile.FullName)

        Remove-Item $tempFile

        $basicFileInfo -eq $loadedBasicFileInfo | Should -BeTrue
        $basicFileInfo._validTypes | Should -HaveCount $loadedBasicFileInfo._validTypes.Count
    }  
}