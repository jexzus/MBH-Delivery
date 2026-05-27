#pragma warning disable CA1416
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Plugin.LocalNotification;

namespace MauiBlazorDelivery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(f => f.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"))
            .UseLocalNotification(config =>
            {
                config.AddAndroid(android =>
                {
                    android.AddChannel(new Plugin.LocalNotification.AndroidOption.NotificationChannelRequest
                    {
                        Id = "delivery_channel",
                        Name = "Notificaciones de Delivery",
                        Importance = Plugin.LocalNotification.AndroidOption.AndroidImportance.High,
                        EnableVibration = true,
                        EnableSound = true,
                        LockScreenVisibility = Plugin.LocalNotification.AndroidOption.AndroidVisibilityType.Public,
                    });
                });
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler<Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView,
                    MauiBlazorDelivery.Platforms.Android.CustomBlazorWebViewHandler>();
#endif
            });

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
        //relaciones de dependecias
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
        builder.Services.AddSingleton<MauiBlazorDelivery.Services.NotificationService>();
        builder.Services.AddSingleton<MauiBlazorDelivery.Services.ImagenCacheService>();

        return builder.Build();
    }

    private static string ResolveBaseUrl()
    {
        // Apuntamos directamente al servidor en la nube para la exposición
        return "http://jamburgers.runasp.net/";

        //#if ANDROID
        //      return "http://192.168.0.13:5224/";   // casa  ← cambiar a 192.168.4.144 en trabajo
        //#elif DEBUG
        //      return "https://localhost:7189/";
        //#else
        //return "https://localhost:7189/";
//#endif
    }
}

