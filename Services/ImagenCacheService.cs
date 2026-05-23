using System.Collections.Concurrent;

namespace MauiBlazorDelivery.Services;

public class ImagenCacheService
{
    private readonly HttpClient _http;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public ImagenCacheService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Api");
    }

    public async Task<string> GetDataUrlAsync(string? imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName)) return "";
        if (_cache.TryGetValue(imageName, out var cached)) return cached;

        try
        {
            var url = $"imagenes/{Uri.EscapeDataString(imageName)}";
            var bytes = await _http.GetByteArrayAsync(url);
            if (bytes.Length == 0) return "";

            var ext = Path.GetExtension(imageName).TrimStart('.').ToLower();
            var mime = ext is "jpg" or "jpeg" ? "image/jpeg" : $"image/{ext}";
            var dataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
            _cache[imageName] = dataUrl;
            return dataUrl;
        }
        catch { return ""; }
    }
}
