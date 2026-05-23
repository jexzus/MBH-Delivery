using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;

namespace MauiBlazorDelivery.Services;

public class NotificationService
{
    private static int _id = 1000;

    public event Action<string>? NotificacionTocada;

    public NotificationService()
    {
        LocalNotificationCenter.Current.NotificationActionTapped += OnTapped;
    }

    private void OnTapped(NotificationActionEventArgs e)
    {
        var data = e.Request?.ReturningData;
        if (!string.IsNullOrEmpty(data))
            MainThread.BeginInvokeOnMainThread(() => NotificacionTocada?.Invoke(data));
    }

    public async Task PedirPermisoAsync()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
        SolicitarExcepcionBateriaYWakeLock();
    }

    private void SolicitarExcepcionBateriaYWakeLock()
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var packageName = context.PackageName!;

            // Pide excepción de optimización de batería (muestra diálogo al usuario una sola vez)
            var pm = (Android.OS.PowerManager)context.GetSystemService(Android.Content.Context.PowerService)!;
            if (!pm.IsIgnoringBatteryOptimizations(packageName))
            {
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse($"package:{packageName}"));
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                context.StartActivity(intent);
            }

            // WakeLock parcial: mantiene la CPU activa para que SignalR no se corte
            if (_wakeLock == null)
            {
                _wakeLock = pm.NewWakeLock(
                    Android.OS.WakeLockFlags.Partial,
                    "JamBurgers::SignalRWakeLock");
                _wakeLock.SetReferenceCounted(false);
                _wakeLock.Acquire();
            }
        }
        catch { }
#endif
    }

#if ANDROID
    private Android.OS.PowerManager.WakeLock? _wakeLock;
#endif

    public void Mostrar(string titulo, string descripcion, string returningData = "")
    {
        var request = new NotificationRequest
        {
            NotificationId = _id++,
            Title = titulo,
            Description = descripcion,
            ReturningData = returningData,
            Android = new AndroidOptions
            {
                ChannelId = "delivery_channel",
                Priority = AndroidPriority.High,
                AutoCancel = true,
                VisibilityType = AndroidVisibilityType.Public,
            }
        };

        MainThread.BeginInvokeOnMainThread(async () =>
            await LocalNotificationCenter.Current.Show(request));
    }
}
