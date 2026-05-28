using System.Net.Http.Headers;

namespace MauiBlazorDelivery.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            catch { /* SecureStorage no disponible en simulador/tests */ }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
