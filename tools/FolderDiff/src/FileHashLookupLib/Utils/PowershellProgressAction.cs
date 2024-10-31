using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Utils;

/// <summary>
/// Based on excellent in-depth knowledge of https://github.com/jborean93
/// </summary>
/// <typeparam name="TProgress">Progress</typeparam>
public class PowershellProgressAction<TProgress> : IProgressAction<TProgress>
    where TProgress : class
{
    private readonly string _script;
    private readonly PSHost _host;

    /// <summary>
    /// Based on excellent in-depth knowledge of https://github.com/jborean93
    /// </summary>
    /// <typeparam name="TProgress"></typeparam>
    public PowershellProgressAction(PSHost host, ScriptBlock sbk)
    {
        _host = host;
        _script = sbk.ToString();
    }

    public void Action(TProgress progress)
    {
        using var rs = RunspaceFactory.CreateRunspace(_host);

        rs.Open();

        using var ps = PowerShell.Create();

        ps.Runspace = rs;

        ps.AddScript(_script).AddArgument(progress);

        try
        {
            ps.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ex.GetType().Name}: {ex.Message}]\n{ex.StackTrace}");
        }
    }
}