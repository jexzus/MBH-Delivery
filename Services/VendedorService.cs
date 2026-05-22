using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class VendedorService
    {
        private readonly HttpClient _http;
        public VendedorService(HttpClient http) => _http = http;

        public class VendedorDto
        {
            public int Id { get; set; }
            public int IdUsuario { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public bool Activo { get; set; }
            public DateTime FechaAlta { get; set; }
            public string? NombreUsuario { get; set; }
        }

        public class VendedorRequest
        {
            public int IdUsuario { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public bool Activo { get; set; } = true;
        }

        public class UsuarioSimpleDto
        {
            public int Id { get; set; }
            public string NombreUsuario { get; set; } = "";
            public string Rol { get; set; } = "";
        }

        public async Task<List<UsuarioSimpleDto>> GetTodosUsuariosAsync()
            => await _http.GetFromJsonAsync<List<UsuarioSimpleDto>>("api/usuarios/todos") ?? new();

        public async Task<List<VendedorDto>> GetTodosAsync()
            => await _http.GetFromJsonAsync<List<VendedorDto>>("api/vendedores") ?? new();

        public async Task<bool> CrearAsync(VendedorRequest req)
        {
            var resp = await _http.PostAsJsonAsync("api/vendedores", req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarAsync(int id, VendedorRequest req)
        {
            var resp = await _http.PutAsJsonAsync($"api/vendedores/{id}", req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/vendedores/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
