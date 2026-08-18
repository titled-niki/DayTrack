using Microsoft.Win32;
namespace DayTrack.Services;
public sealed class AutoStartService
{
 const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run", Name="DayTrack";
 public void SetEnabled(bool enabled){ using var k=Registry.CurrentUser.OpenSubKey(RunKey,true)??Registry.CurrentUser.CreateSubKey(RunKey); if(enabled){var exe=Environment.ProcessPath??""; k.SetValue(Name,$"\"{exe}\" --startup");} else k.DeleteValue(Name,false); }
}
