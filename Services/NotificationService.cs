using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace MauiBlazorDelivery.Services;

public class NotificationService
{
    private static int _id = 1000;

    public async Task PedirPermisoAsync()
    {
        await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public void Mostrar(string titulo, string descripcion)
    {
        var request = new NotificationRequest
        {
            NotificationId = _id++,
            Title = titulo,
            Description = descripcion,
            Android = new AndroidOptions
            {
                ChannelId = "delivery_channel",
                Priority = AndroidPriority.High,
                AutoCancel = true,
            }
        };

        MainThread.BeginInvokeOnMainThread(async () =>
            await LocalNotificationCenter.Current.Show(request));
    }
}
