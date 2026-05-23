using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiBlazorDelivery.Platforms.Android;

public class CustomBlazorWebViewHandler : BlazorWebViewHandler
{
    protected override void ConnectHandler(global::Android.Webkit.WebView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Settings.MixedContentMode = global::Android.Webkit.MixedContentHandling.AlwaysAllow;
    }
}
