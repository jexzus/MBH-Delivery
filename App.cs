using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiBlazorDelivery;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var contentPage = new ContentPage();
        var blazorWebView = new BlazorWebView
        {
            HostPage = "wwwroot/index.html",
            // 👇 Arranca SIEMPRE en /login
            StartPath = "/login"
        };

        blazorWebView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Components.App)
        });

        contentPage.Content = blazorWebView;

        return new Window(contentPage)
        {
            Title = "MauiBlazorDelivery"
        };
    }
}
