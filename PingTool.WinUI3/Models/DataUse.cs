using CommunityToolkit.Mvvm.ComponentModel;

namespace PingTool.Models;

public partial class DataUse : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private ulong _upload;

    [ObservableProperty]
    private ulong _download;

    [ObservableProperty]
    private TimeSpan _connectionDuration;
}

public class NetworkDataUse
{
    public string DataType { get; set; } = string.Empty;
    public ulong DataUse { get; set; }
}
