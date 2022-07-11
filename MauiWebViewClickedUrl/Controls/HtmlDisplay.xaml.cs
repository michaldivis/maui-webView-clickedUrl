using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MauiWebViewClickedUrl.Controls;

public partial class HtmlDisplay : ContentView
{
    private const string _baseUrl = "www.baseUrl.org/test/";

    public static readonly BindableProperty HtmlContentProperty = BindableProperty.Create(nameof(HtmlContent), typeof(string), typeof(HtmlDisplay), propertyChanged: OnHtmlContentChanged);
    public string HtmlContent
    {
        get => GetValue(HtmlContentProperty) as string;
        set => SetValue(HtmlContentProperty, value);
    }

    public static readonly BindableProperty LinkClickedCommandProperty = BindableProperty.Create(nameof(LinkClickedCommand), typeof(ICommand), typeof(HtmlDisplay));
    public ICommand LinkClickedCommand
    {
        get => GetValue(LinkClickedCommandProperty) as ICommand;
        set => SetValue(LinkClickedCommandProperty, value);
    }

    public HtmlDisplay()
    {
        InitializeComponent();

        TheWebView.Navigating += TheWebView_Navigating;
    }

    private static void OnHtmlContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HtmlDisplay htmlDisplay)
        {
            if (newValue is string html)
            {
                //set html content
                htmlDisplay.SetHtmlSource(html);
            }
            else
            {
                //clear content
                htmlDisplay.SetHtmlSource(null);
            }
        }
    }

    private void SetHtmlSource(string html)
    {
        var source = new HtmlWebViewSource()
        {
            BaseUrl = _baseUrl,
            Html = html
        };
        TheWebView.Source = source;
    }

    private void TheWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.NavigationEvent != WebNavigationEvent.NewPage)
        {
            //unwanted navigation, I only care about NewPage
            e.Cancel = true;
            return;
        }

        var link = GetLinkForCurrentPlatform(e);

        if (link is null)
        {
            //unknown link
            e.Cancel = true;
            return;
        }

        TryExecuteLinkClickedCommand(link);
    }

    private string GetLinkForCurrentPlatform(WebNavigatingEventArgs e)
    {
        if(DeviceInfo.Current.Platform == DevicePlatform.Android)
        {
            return GetLink_Android(e);
        }

        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            return GetLink_iOS(e);
        }

        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            return GetLink_WinUI(e);
        }

        throw new NotImplementedException();
    }

    private string GetLink_Android(WebNavigatingEventArgs e)
    {
        //no way to get the url
        _ = e.Url; //this is equal to about:blank#blocked
        return null;
    }

    private string GetLink_iOS(WebNavigatingEventArgs e)
    {
        var match = Regex.Match(e.Url, $@".*{Regex.Escape(_baseUrl)}(?<link>.*)");
        if (match.Success is false)
        {
            return null;
        }

        var link = match.Groups["link"].Value;

        return link;
    }

    private string GetLink_WinUI(WebNavigatingEventArgs e)
    {
        var invalidMatch = Regex.Match(e.Url, @"about:blank.*");
        if (invalidMatch.Success)
        {
            return null;
        }

        var match = Regex.Match(e.Url, @"about:(?<link>.*)");
        if (match.Success is false)
        {
            return null;
        }

        var link = match.Groups["link"].Value;

        return link;
    }

    private void TryExecuteLinkClickedCommand(string link)
    {
        if (LinkClickedCommand.CanExecute(link))
        {
            LinkClickedCommand.Execute(link);
        }
    }
}