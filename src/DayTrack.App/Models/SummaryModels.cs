namespace DayTrack.Models;
public sealed class RangeSummary
{
 public double ActiveSeconds { get; set; }
 public double AfkSeconds { get; set; }
 public double LockedSeconds { get; set; }
 public double SleepSeconds { get; set; }
 public double PausedSeconds { get; set; }
 public double PcOnSeconds { get; set; }
 public long KeyPresses { get; set; }
 public long MouseClicks { get; set; }
 public long NetworkReceived { get; set; }
 public long NetworkSent { get; set; }
}
public sealed class AppUsageRow
{
 public string AppName { get; set; } = "";
 public double ActiveSeconds { get; set; }
 public long Launches { get; set; }
}
public sealed class TrackerSnapshot
{
 public string State { get; init; } = "active";
 public string AppName { get; init; } = "";
 public double TodayActiveSeconds { get; init; }
 public long TodayKeyPresses { get; init; }
 public long TodayMouseClicks { get; init; }
 public long TodayNetworkReceived { get; init; }
 public long TodayNetworkSent { get; init; }
 public bool IsPaused { get; init; }
}
