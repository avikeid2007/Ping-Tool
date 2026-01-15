using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Net.NetworkInformation;

namespace PingTool.Converters;

/// <summary>
/// Converts NetworkInterface OperationalStatus to a brush color for the icon
/// </summary>
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OperationalStatus status)
        {
            return status == OperationalStatus.Up
                ? new SolidColorBrush(ColorHelper.FromArgb(255, 16, 185, 129)) // Green
                : new SolidColorBrush(ColorHelper.FromArgb(255, 156, 163, 175)); // Gray
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts NetworkInterface OperationalStatus to a border brush
/// </summary>
public class StatusToBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OperationalStatus status)
        {
            return status == OperationalStatus.Up
                ? new SolidColorBrush(ColorHelper.FromArgb(50, 16, 185, 129)) // Green transparent
                : new SolidColorBrush(Colors.Transparent);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts NetworkInterface OperationalStatus to background color for status badge
/// </summary>
public class StatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OperationalStatus status)
        {
            return status == OperationalStatus.Up
                ? new SolidColorBrush(ColorHelper.FromArgb(255, 16, 185, 129)) // Green
                : new SolidColorBrush(ColorHelper.FromArgb(255, 107, 114, 128)); // Gray
        }
        return new SolidColorBrush(ColorHelper.FromArgb(255, 107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts NetworkInterface OperationalStatus to opacity (disconnected = faded)
/// </summary>
public class StatusToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is OperationalStatus status)
        {
            return status == OperationalStatus.Up ? 1.0 : 0.6;
        }
        return 0.6;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts double to formatted string with one decimal place
/// </summary>
public class DoubleToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
        {
            return d.ToString("F1");
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
