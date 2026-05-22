using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;

namespace MauiBlazorDelivery;

public class App : Application
{
    public App()
    {
        var blazor = new BlazorWebView
        {
            HostPage = "wwwroot/index.html",
            StartPath = "/login",
        };

        blazor.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Components.App)
        });

        MainPage = new ContentPage { Content = blazor };
    }
}
