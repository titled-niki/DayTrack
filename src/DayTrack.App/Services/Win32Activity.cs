using System.Diagnostics; using System.Runtime.InteropServices; using System.Text;
namespace DayTrack.Services;
public static class Win32Activity
{
 [StructLayout(LayoutKind.Sequential)] struct LASTINPUTINFO{public uint cbSize; public uint dwTime;}
 [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr hWnd,StringBuilder text,int count);
 [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
 public static (string App,string Process,string Title) GetForegroundApp(bool title){ try{var h=GetForegroundWindow(); GetWindowThreadProcessId(h,out var pid); using var p=Process.GetProcessById((int)pid); string t=""; if(title){var sb=new StringBuilder(1024); GetWindowText(h,sb,sb.Capacity); t=sb.ToString();} return(FriendlyName(p.ProcessName),p.ProcessName,t);}catch{return("Unknown","","");} }
 public static TimeSpan GetIdleTime(){var i=new LASTINPUTINFO{cbSize=(uint)Marshal.SizeOf<LASTINPUTINFO>()}; if(!GetLastInputInfo(ref i)) return TimeSpan.Zero; long d=Environment.TickCount64-i.dwTime; if(d<0)d+=uint.MaxValue; return TimeSpan.FromMilliseconds(d);}
 public static string FriendlyName(string p)=>p.ToLowerInvariant() switch{"chrome"=>"Google Chrome","msedge"=>"Microsoft Edge","firefox"=>"Mozilla Firefox","explorer"=>"File Explorer","code"=>"Visual Studio Code","robloxplayerbeta"=>"Roblox","telegram"=>"Telegram","discord"=>"Discord","steam"=>"Steam","devenv"=>"Visual Studio","pycharm64"=>"PyCharm","winword"=>"Microsoft Word","excel"=>"Microsoft Excel","powerpnt"=>"PowerPoint",_=>p};
}
