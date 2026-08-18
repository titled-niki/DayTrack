using System.Net.NetworkInformation;
namespace DayTrack.Services;
public sealed class NetworkTracker
{
 readonly Dictionary<string,(long Rx,long Tx)> _last=new();
 public void ResetBaseline(){_last.Clear();foreach(var x in Snap())_last[x.Key]=x.Value;}
 public (long Rx,long Tx) SampleDelta(){long rx=0,tx=0;var n=Snap();foreach(var x in n){if(_last.TryGetValue(x.Key,out var o)){rx+=Math.Max(0,x.Value.Rx-o.Rx);tx+=Math.Max(0,x.Value.Tx-o.Tx);}_last[x.Key]=x.Value;}return(rx,tx);}
 static Dictionary<string,(long Rx,long Tx)> Snap(){var r=new Dictionary<string,(long,long)>();foreach(var n in NetworkInterface.GetAllNetworkInterfaces()){try{if(n.OperationalStatus!=OperationalStatus.Up||n.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)continue;var s=n.GetIPv4Statistics();r[n.Id]=(s.BytesReceived,s.BytesSent);}catch{}}return r;}
}
