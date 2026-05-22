#pragma warning disable CA1416
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiBlazorDelivery;

public static class MauiProgram
{
    public static readonly bool DemoMode = false;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(f => f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddSingleton<MauiBlazorDelivery.Services.AppState>();

        var baseUrl = ResolveBaseUrl();
        builder.Services.AddSingleton(new MauiBlazorDelivery.Services.SignalRService(baseUrl));
        Console.WriteLine($"[DEBUG] Using base URL: {baseUrl}");

        builder.Services.AddHttpClient("Api", c =>
        {
            c.BaseAddress = new Uri(baseUrl);      // ¡con "/" final!
            c.Timeout = TimeSpan.FromSeconds(20);
            c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
#if DEBUG
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
#else
            return new HttpClientHandler();
#endif
        });

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

        builder.Services.AddScoped<MauiBlazorDelivery.Services.AuthService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.PedidosService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.ClienteService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.ProductoService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.AdminService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.VendedorService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.RepartidorService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.PreRegistroService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.RecuperarContrasenaService>();

        return builder.Build();
    }

    private static string ResolveBaseUrl()
    {
#if ANDROID
        return "http://192.168.0.13:5224/";   // casa  ← cambiar a 192.168.4.144 en trabajo
#elif DEBUG
        return "https://localhost:7189/";
#else
        return "https://localhost:7189/";
#endif
    }
}

