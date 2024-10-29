using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

public class ProgressAction
{
    private PSHost _host;
    private IDictionary _state;
    private string _script;

    public ProgressAction(PSHost host, IDictionary state, ScriptBlock sbk)
    {
        // Store the current host and script, the host is used
        // so that Write-Host will work.
        _host = host;
        _state = state;
        _script = sbk.ToString();
    }

    public void Action(string name)
    {
        // When the delegate is invoked, create the Runspace
        // to run our script and pass in the argument;
        using (Runspace rs = RunspaceFactory.CreateRunspace(_host))
        {
            rs.Open();

            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;
            ps.AddScript(_script).AddArgument(name).AddArgument(_state);

            // This discards any output and does no error
            // handling, you may want to change this

            ps.Invoke();
        }
    }
}