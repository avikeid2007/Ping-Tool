using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PingTool.Helpers;

public static class FileHelper
{
    public static void CopyText(string text)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(text);
        Clipboard.SetContent(dataPackage);
    }

    public static async Task SaveFileAsync(string content, string suggestedFileName)
    {
        var savePicker = new FileSavePicker();
        
        // Get the window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Text Files", new List<string> { ".txt" });
        savePicker.SuggestedFileName = suggestedFileName;

        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            await FileIO.WriteTextAsync(file, content);
        }
    }
}
