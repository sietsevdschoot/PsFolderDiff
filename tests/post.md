Hello Redditors!

I have a C# application which reports progress using `[Progress[T]]`. 
Using Powershell I want to supply an `Action[T]` defining how progress should be displayed. 

This is were I run into problems with runspaces and synchronization. 

The issue can be simplified down to the following code:

    using namespace System;

    $myAction = ([Action[string]]{ param($name) Write-Host "Hello world: $name" })

    $myAction.Invoke("Foo")
    # This works fine. Output Hello world: Foo
    
    $myProgress = [Progress[string]]::new($myAction)
    
    $myProgress.Report("Foo")
    
    # This gives the following error:
    # Unhandled exception. System.Management.Automation.PSInva1idOperationException:
    # There is no Runspace available to run scripts in this thread.
    # You can provide one in the DefaultRunspace property of the System.Management.Automation.Runspaces.Runspace type. 

