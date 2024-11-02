using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

public class ProgressAction<TProgress>
    where TProgress : class
{
    private PSHost _host;
    private string _script;

    public ProgressAction(PSHost host, ScriptBlock sbk)
    {
        // Store the current host and script, the host is used
        // so that Write-Host will work.
        _host = host;
        _script = sbk.ToString();
    }

    public void Action(TProgress progress)
    {
        // When the delegate is invoked, create the Runspace
        // to run our script and pass in the argument;
        using (Runspace rs = RunspaceFactory.CreateRunspace(_host))
        {
            rs.Open();

            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;

            ps.AddScript(_script).AddArgument(progress);

            // This discards any output and does no error
            // handling, you may want to change this

            try {

                ps.Invoke();
            }
            catch {

                throw;
            }
            finally {

                ps.Stop();
                rs.Close();
            }
        }
    }
}