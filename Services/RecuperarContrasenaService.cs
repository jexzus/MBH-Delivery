using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class RecuperarContrasenaService
    {
        private readonly HttpClient _http;
        public RecuperarContrasenaService(HttpClient http) => _http = http;

        public async Task<(bool ok, string? error)> SolicitarCodigoAsync(string email)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/usuarios/solicitar-recuperacion", new { Email = email });
                if (resp.IsSuccessStatusCode) return (true, null);
                var body = await resp.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)resp.StatusCode}" : body);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool ok, string? error)> CambiarContrasenaAsync(string email, string codigo, string nuevaContrasena)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/usuarios/cambiar-contrasena",
                    new { Email = email, Codigo = codigo, NuevaContrasena = nuevaContrasena });
                if (resp.IsSuccessStatusCode) return (true, null);
                var body = await resp.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(body) ? $"Error {(int)resp.StatusCode}" : body);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
