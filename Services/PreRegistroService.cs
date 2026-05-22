using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class PreRegistroService
    {
        private readonly HttpClient _http;
        public PreRegistroService(HttpClient http) => _http = http;

        public async Task<(bool ok, string? error)> EnviarCodigoAsync(string email)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/usuarios/pre-registro", new { Email = email });
                if (resp.IsSuccessStatusCode) return (true, null);
                var body = await resp.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)resp.StatusCode}" : body);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string? error)> ConfirmarCodigoAsync(string email, string codigo)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/usuarios/confirmar-pre-registro",
                    new { Email = email, Codigo = codigo });
                if (resp.IsSuccessStatusCode) return (true, null);
                var body = await resp.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)resp.StatusCode}" : body);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
