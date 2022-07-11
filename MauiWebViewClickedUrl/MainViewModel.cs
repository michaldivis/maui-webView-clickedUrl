using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiWebViewClickedUrl;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _htmlCode;

    [RelayCommand]
    private void ReloadHtmlCode()
    {
        HtmlCode = Properties.Resources.example;
    }

    [RelayCommand]
    private async Task HtmlLinkClickedAsync(string link)
    {
        await Application.Current.MainPage.DisplayAlert("Link clicked", link, "OK");
    }
}
