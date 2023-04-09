
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

var perfData = File.ReadAllLines("perf64.log").Skip(1)
    .ToDictionary(
        o => DateTime.Parse(o.Split('\t').First().Trim('\"')).ToString("HH:mm:ss"),
        o => string.Join(",",
            o.Split('\t').Skip(1).Select(s => Convert.ToInt32(float.Parse(s.Trim('\"')))).ToArray()));
var iterationLines = File.ReadAllLines("result.json")
    .SkipWhile(o => !o.StartsWith("{\"metric\":\"iterations"));
var stats = new Dictionary<int, StatsPoint>();
foreach (string line in iterationLines)
{
    var item = JsonObject.Parse(line);
    var metric = item["metric"].GetValue<string>();
    if (metric != "http_req_duration") continue;
    var data = item["data"].AsObject();
    var logTime = data["time"].GetValue<DateTime>();
    var val = Convert.ToInt32(data["value"].GetValue<float>());
    var tags = data["tags"].AsObject();
    var startTime = tags["timestamp"].GetValue<DateTime>();
    var status = tags["status"].GetValue<string>();
    var stageProfile = tags["stage_profile"].GetValue<string>();
    var target = tags["target"].GetValue<string>();
    if (StatsPoint.StartTime == DateTime.MinValue)
        StatsPoint.StartTime = startTime;
    var secs = Convert.ToInt32((logTime - StatsPoint.StartTime).TotalSeconds);
    if (!stats.ContainsKey(secs)) stats.Add(secs, new StatsPoint { Time = secs });
    stats[secs].ReqSent++;
    stats[secs].Stage = stageProfile == "steady" ? target : $"{target}";
    var doneSecs = secs + Convert.ToInt32(val / 1000);
    if (!stats.ContainsKey(doneSecs)) stats.Add(doneSecs, new StatsPoint { Time = doneSecs });
    if (status == "200")
    {
        stats[doneSecs].ReqDone++;
        stats[doneSecs].DoneDura += val;
    }
    else
    {
        stats[doneSecs].FailedDura += val;
        stats[doneSecs].ReqFailed++;
        if (!stats[doneSecs].ErrCodes.ContainsKey(status))
            stats[doneSecs].ErrCodes.Add(status, 1);
        else
            stats[doneSecs].ErrCodes[status]++;
    }
}

Console.WriteLine(
    "Time,TimeSpan,Stage,ReqSent,ReqDone,ReqFailed,AvgDoneDura,AvgFailedDura,AvgDura,ErrCodeStats,Cpu,Mem,Disk");

foreach (var time in stats.Keys.OrderBy(o => o))
{
    var stat = stats[time];
    Console.WriteLine(string.Join(",", new string[]
    {
        stat.LogTime, stat.TimeSpan.ToString("mm\\:ss"), stat.Stage,
        stat.ReqSent.ToString(), stat.ReqDone.ToString(), stat.ReqFailed.ToString(),
        stat.AvgDoneDura.ToString(), stat.AvgFailedDura.ToString(), stat.AvgDura.ToString(),
        stat.ErrCodeStats,
        perfData.ContainsKey(stat.LogTime) ? perfData[stat.LogTime] : ",,"
    }));
}

public class StatsPoint
{
    public static DateTime StartTime = DateTime.MinValue;
    public int Time { get; set; }
    public TimeSpan TimeSpan => TimeSpan.FromSeconds(Time);
    public string LogTime => StartTime.AddSeconds(Time).ToLocalTime().ToString("HH:mm:ss");
    public string Stage { get; set; }
    public int ReqSent { get; set; }
    public int ReqDone { get; set; }
    public int ReqFailed { get; set; }
    public Dictionary<string, int> ErrCodes = new Dictionary<string, int>();
    public string ErrCodeStats => string.Join("|", ErrCodes.Select(o => $"{o.Key}:{o.Value}"));
    public int ReqExec => ReqDone + ReqFailed;
    public long DoneDura { get; set; }
    public long FailedDura { get; set; }
    public long TotalDuration => DoneDura + FailedDura;
    public int AvgDura => ReqExec > 0 ? Convert.ToInt32(TotalDuration / ReqExec) : 0;
    public int AvgDoneDura => ReqDone > 0 ? Convert.ToInt32(DoneDura / ReqDone) : 0;
    public int AvgFailedDura => ReqFailed > 0 ? Convert.ToInt32(FailedDura / ReqFailed) : 0;
}

