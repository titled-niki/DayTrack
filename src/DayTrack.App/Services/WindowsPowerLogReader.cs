using System.Diagnostics.Eventing.Reader;
namespace DayTrack.Services;
public sealed class PowerDayStats{public int Boots{get;set;}public int Shutdowns{get;set;}public int Unexpected{get;set;}}
public sealed class WindowsPowerLogReader
{
 public PowerDayStats GetDay(DateTime day){var x=new PowerDayStats();try{string s=day.Date.ToUniversalTime().ToString("o"),e=day.Date.AddDays(1).ToUniversalTime().ToString("o");string xp=$"*[System[(EventID=6005 or EventID=6006 or EventID=6008) and TimeCreated[@SystemTime>='{s}' and @SystemTime<'{e}']]]";using var r=new EventLogReader(new EventLogQuery("System",PathType.LogName,xp));for(EventRecord? z=r.ReadEvent();z!=null;z=r.ReadEvent()){using(z){if(z.Id==6005)x.Boots++;else if(z.Id==6006)x.Shutdowns++;else if(z.Id==6008)x.Unexpected++;}}}catch{}return x;}
}
