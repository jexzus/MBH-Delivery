using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MauiBlazorDelivery.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        public UsuarioLogin? UsuarioActual { get; private set; }

        public AuthService(HttpClient http) => _http = http;

        public async Task<UsuarioLogin?> LoginAsync(string username, string password)
        {
            var request = new { NombreUsuario = username, Contraseña = password };
            var response = await _http.PostAsJsonAsync("api/usuarios/login", request);
            if (!response.IsSuccessStatusCode) return null;

            // debug opcional
            var contenido = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG AuthService] Login → {contenido}");

            UsuarioActual = await response.Content.ReadFromJsonAsync<UsuarioLogin>();
            return UsuarioActual;
        }

        /// <summary>
        /// Alta de cliente contra POST api/usuarios/registrar
        /// </summary>
        public async Task<(bool ok, string? error)> RegistrarClienteAsync(RegistroClienteDto dto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/usuarios/registrar", dto);
                if (resp.IsSuccessStatusCode) return (true, null);

                var body = await resp.Content.ReadAsStringAsync();
                var msg = string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)resp.StatusCode}" : body;
                return (false, msg);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Si tu backend viejo devuelve "id" la app igual funciona.
        public int IdUsuario =>
            (UsuarioActual?.IdUsuario ?? 0) != 0
                ? UsuarioActual!.IdUsuario
                : (UsuarioActual?.IdAlternativo ?? 0);

        public bool EsAdmin => UsuarioActual?.Rol?.Trim().ToLower() == "admin";
        public bool EsCliente => UsuarioActual?.Rol?.Trim().ToLower() == "cliente";
        public bool EstaLogueado => UsuarioActual is not null;

        public void Logout() => UsuarioActual = null;
    }

    public class UsuarioLogin
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        // tolerancia si el backend devuelve "id"
        [JsonPropertyName("id")]
        public int IdAlternativo { get; set; }

        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;
    }

    /// DTO para registrar cliente (serializa con las claves que espera tu API)
    public class RegistroClienteDto
    {
        [JsonPropertyName("NombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        // usamos "Contrasena" en C#, pero serializamos como "Contraseña"
        [JsonPropertyName("Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [JsonPropertyName("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [JsonPropertyName("NumTelefono")]
        public string NumTelefono { get; set; } = string.Empty;

        [JsonPropertyName("Domicilio")]
        public string Domicilio { get; set; } = string.Empty;
    }
}
