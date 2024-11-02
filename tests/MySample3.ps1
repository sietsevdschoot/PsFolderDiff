Add-Type -TypeDefinition (Get-Content $PSScriptRoot\Code\ProgressAction.cs -raw) 

$myDelegate = [ProgressAction[string]]::new($host, {
    param($name)
    
    Write-Host "Hello world: $name" -ForegroundColor Green
})

$myProgress = [Progress[string]]::new($myDelegate.Action)
    
$myProgress.Report("Foo1")
$myProgress.Report("Foo2")
$myProgress.Report("Foo3")
$myProgress.Report("Foo4")
