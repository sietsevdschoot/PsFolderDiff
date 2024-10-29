Add-Type -TypeDefinition (Get-Content $PSScriptRoot\Code\ProgressAction.cs -raw) 

$myState = @{}

$myDelegate = [ProgressAction]::new($host, $myState, {
    param($name, $state)
    
    Write-Host "Hello world: $name" -ForegroundColor Green
})

$myProgress = [Progress[string]]::new($myDelegate.Action)
    
$myProgress.Report("Foo")
