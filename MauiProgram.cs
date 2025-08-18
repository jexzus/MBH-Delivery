using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiBlazorDelivery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // ✅ ESTADO GLOBAL
        builder.Services.AddSingleton<MauiBlazorDelivery.Services.AppState>();

        // ✅ BASE URL POR PLATAFORMA
        var baseUrl = GetBaseUrl();

        // ✅ HTTP CLIENT FACTORY
        builder.Services
            .AddHttpClient("Api", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(100);
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
            })
#endif
            ;

        // ✅ HTTP CLIENT POR DEFECTO
        builder.Services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient("Api");
        });

        // ✅ TODOS LOS SERVICIOS (SIEMPRE, NO SOLO EN DEBUG)
        builder.Services.AddScoped<MauiBlazorDelivery.Services.AuthService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.ProductoService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.PedidosService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.ClienteService>();
        builder.Services.AddScoped<MauiBlazorDelivery.Services.AdminService>(); // ⭐ MOVIDO FUERA DEL #if DEBUG

        // ✅ CONFIG OPCIONAL
        builder.Services.Configure<ApiSettings>(o => o.BaseUrl = baseUrl.TrimEnd('/'));

        return builder.Build();
    }

    private static string GetBaseUrl()
    {
        const string LOCAL_IP = "192.168.1.105"; // tu IP LAN (para Android/iOS si hace falta)

#if WINDOWS || MACCATALYST
    return "https://localhost:7189/";        // 👈 coincide con el certificado dev
#elif ANDROID || IOS
        return $"https://{LOCAL_IP}:7189/";
#else
    return $"https://{LOCAL_IP}:7189/";
#endif
    }

}

public class ApiSettings
{ public string BaseUrl { get; set; } = string.Empty; }