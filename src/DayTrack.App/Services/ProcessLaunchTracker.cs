using System.Diagnostics;
namespace DayTrack.Services;
public sealed class ProcessLaunchTracker
{
 HashSet<int> _known=new();
 public void ResetBaseline(){_known=Process.GetProcesses().Select(p=>{try{return p.Id;}finally{p.Dispose();}}).ToHashSet();}
 public Dictionary<string,int> Sample(){var r=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);var now=new HashSet<int>();foreach(var p in Process.GetProcesses()){try{now.Add(p.Id);if(_known.Contains(p.Id)||p.MainWindowHandle==IntPtr.Zero)continue;var n=Win32Activity.FriendlyName(p.ProcessName);r[n]=r.TryGetValue(n,out var c)?c+1:1;}catch{}finally{p.Dispose();}}_known=now;return r;}
}
