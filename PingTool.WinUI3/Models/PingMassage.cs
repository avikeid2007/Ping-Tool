using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace PingTool.Models;

public partial class PingMassage : ObservableObject
{
    [ObservableProperty]
    [property: PrimaryKey, AutoIncrement]
    private int _id;

    [ObservableProperty]
    private Guid _pingId;

    [ObservableProperty]
    private string _ipAddress = string.Empty;

    [ObservableProperty]
    private int _size;

    [ObservableProperty]
    private long _time;

    [ObservableProperty]
    private int _ttl;

    [ObservableProperty]
    private string _response = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _date;
}
