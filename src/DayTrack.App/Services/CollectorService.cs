using DayTrack.Data; using DayTrack.Models; using Microsoft.Win32;
namespace DayTrack.Services;
public sealed class CollectorService:IDisposable
{
 readonly DayTrackDb _db; readonly SettingsService _settings; readonly ExportService _export; readonly InputCounter _input=new(); readonly NetworkTracker _network=new(); readonly ProcessLaunchTracker _launches=new(); readonly object _gate=new(); System.Threading.Timer? _timer;
 DateTime _day=DateTime.Today,_lastTick=DateTime.Now,_lastNetwork=DateTime.MinValue,_lastLaunch=DateTime.MinValue,_lastFlush=DateTime.MinValue,_lastExport=DateTime.MinValue; long _lastKeys,_lastClicks; double _active,_afk,_locked,_sleep,_paused,_pc; long _keys,_clicks,_rx,_tx; readonly Dictionary<string,double> _apps=new(StringComparer.OrdinalIgnoreCase); readonly Dictionary<string,long> _starts=new(StringComparer.OrdinalIgnoreCase); bool _lockedNow,_sleeping,_pauseForever; DateTime? _sleepStart,_pauseUntil; string _state="active",_app="";
 public event Action<TrackerSnapshot>? SnapshotChanged;
 public CollectorService(DayTrackDb db,SettingsService settings,ExportService export){_db=db;_settings=settings;_export=export;}
 public void Start(){lock(_gate){_input.Start();_lastKeys=_input.KeyPresses;_lastClicks=_input.MouseClicks;_network.ResetBaseline();_launches.ResetBaseline();Hook();_timer=new System.Threading.Timer(_=>Tick(),null,TimeSpan.Zero,TimeSpan.FromSeconds(1));}}
 public bool IsPaused{get{if(_pauseForever)return true;if(_pauseUntil is null)return false;if(DateTime.Now<_pauseUntil.Value)return true;_pauseUntil=null;return false;}}
 public void PauseFor(TimeSpan? d){lock(_gate){_pauseForever=d is null;_pauseUntil=d is null?null:DateTime.Now.Add(d.Value);DiscardInput();}}
 public void Resume(){lock(_gate){_pauseForever=false;_pauseUntil=null;DiscardInput();_network.ResetBaseline();}}
 void Tick(){lock(_gate){try{var now=DateTime.Now;if(now.Date!=_day){Flush();try{_export.DailyTxt(_day);}catch{}ResetDay(now.Date);}double elapsed=Math.Clamp((now-_lastTick).TotalSeconds,0,3);_lastTick=now;if(!_sleeping){_pc+=elapsed;if(IsPaused){_state="paused";_paused+=elapsed;_app="";Input(false);}else if(_lockedNow){_state="locked";_locked+=elapsed;_app="";Input(false);}else if(Win32Activity.GetIdleTime().TotalSeconds>=Math.Max(30,_settings.Current.AfkSeconds)){_state="afk";_afk+=elapsed;_app="";Input(false);}else{_state="active";var f=Win32Activity.GetForegroundApp(_settings.Current.TrackWindowTitles);_app=f.App;_active+=elapsed;if(!string.IsNullOrWhiteSpace(_app))_apps[_app]=_apps.TryGetValue(_app,out var x)?x+elapsed:elapsed;Input(true);}}
 if((now-_lastNetwork).TotalSeconds>=5){if(_settings.Current.TrackNetwork&&!IsPaused){var d=_network.SampleDelta();_rx+=d.Rx;_tx+=d.Tx;}else _network.ResetBaseline();_lastNetwork=now;}
 if((now-_lastLaunch).TotalSeconds>=2){if(!IsPaused){foreach(var x in _launches.Sample())_starts[x.Key]=_starts.TryGetValue(x.Key,out var n)?n+x.Value:x.Value;}else _launches.ResetBaseline();_lastLaunch=now;}
 if((now-_lastFlush).TotalSeconds>=10)Flush(); if((now-_lastExport).TotalSeconds>=60){try{_export.DailyTxt(_day);}catch{} _lastExport=now;}Publish();}catch{}}}
 void Input(bool record){long k=_input.KeyPresses,c=_input.MouseClicks,dk=Math.Max(0,k-_lastKeys),dc=Math.Max(0,c-_lastClicks);if(record){if(_settings.Current.TrackKeyPresses)_keys+=dk;if(_settings.Current.TrackMouseClicks)_clicks+=dc;}_lastKeys=k;_lastClicks=c;}
 void DiscardInput()=>Input(false);
 void Flush(){_db.Add(_day,_active,_afk,_locked,_sleep,_paused,_pc,_keys,_clicks,_rx,_tx,new Dictionary<string,double>(_apps),new Dictionary<string,long>(_starts));_active=_afk=_locked=_sleep=_paused=_pc=0;_keys=_clicks=_rx=_tx=0;_apps.Clear();_starts.Clear();_lastFlush=DateTime.Now;}
 void ResetDay(DateTime d){_day=d;_active=_afk=_locked=_sleep=_paused=_pc=0;_keys=_clicks=_rx=_tx=0;_apps.Clear();_starts.Clear();DiscardInput();_network.ResetBaseline();_launches.ResetBaseline();}
 void Publish(){var s=_db.Summary(_day,_day);SnapshotChanged?.Invoke(new(){State=_state,AppName=_app,TodayActiveSeconds=s.ActiveSeconds+_active,TodayKeyPresses=s.KeyPresses+_keys,TodayMouseClicks=s.MouseClicks+_clicks,TodayNetworkReceived=s.NetworkReceived+_rx,TodayNetworkSent=s.NetworkSent+_tx,IsPaused=IsPaused});}
 void Hook(){try{SystemEvents.SessionSwitch+=Switch;SystemEvents.PowerModeChanged+=Power;SystemEvents.SessionEnding+=Ending;}catch{}}
 void Unhook(){try{SystemEvents.SessionSwitch-=Switch;SystemEvents.PowerModeChanged-=Power;SystemEvents.SessionEnding-=Ending;}catch{}}
 void Switch(object s,SessionSwitchEventArgs e){lock(_gate){if(e.Reason==SessionSwitchReason.SessionLock)_lockedNow=true;else if(e.Reason==SessionSwitchReason.SessionUnlock)_lockedNow=false;}}
 void Power(object s,PowerModeChangedEventArgs e){lock(_gate){if(e.Mode==PowerModes.Suspend){Flush();_sleepStart=DateTime.Now;_sleeping=true;}else if(e.Mode==PowerModes.Resume){var end=DateTime.Now; var start=_sleepStart??end;AddSleep(start,end);_sleeping=false;_sleepStart=null;_lastTick=end;_network.ResetBaseline();_launches.ResetBaseline();}}}
 void AddSleep(DateTime start,DateTime end){for(var c=start;c<end;){var n=c.Date.AddDays(1);var pe=end<n?end:n;double sec=Math.Max(0,(pe-c).TotalSeconds);_db.Add(c.Date,0,0,0,sec,0,sec,0,0,0,0,new Dictionary<string,double>(),new Dictionary<string,long>());c=pe;}}
 void Ending(object s,SessionEndingEventArgs e){lock(_gate){Flush();try{_export.DailyTxt(_day);}catch{}}}
 public void Dispose(){lock(_gate){_timer?.Dispose();_timer=null;Flush();try{_export.DailyTxt(_day);}catch{}Unhook();_input.Dispose();}}
}
