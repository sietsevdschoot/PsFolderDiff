using namespace System;

 
# [powershell]::Create().AddScript({ param($myName) Write-Output "Hello world: $myName"}).AddParameter("myName", "Sietse").Invoke()

# $ctx = [System.Threading.SynchronizationContext]::new()


$myAction = ([Action[string]]{ 
    param([string] $name)

    $runspace = [RunspaceFactory]::CreateRunspace()
    $runspace.Open()
    # Use the runspace to invoke the PowerShell command
    $ps = [powershell]::Create()
    $ps.Runspace = $runspace
    $ps.AddScript({ param($myName) Write-Output "Hello world: $myName"})
    $ps.AddParameter("myName", "Sietse")
    $ps.Invoke()
})

$name = "Foo"
try {

    $myAction.Invoke("Sietse")

    $myProgress = [Progress[string]]::new($myAction)

    $myProgress.Report("Sietse")
}
catch {

    $_ | Out-String | Write-Host

    Start-Sleep -Seconds 10
}


