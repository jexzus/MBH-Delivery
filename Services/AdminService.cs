using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class AdminService
    {
        private readonly HttpClient _http;
        public AdminService(HttpClient http) => _http = http;

        // Cambiar record a class con propiedades mutables
        public class AdminDto
        {
            public int Id { get; set; }
            public string NombreUsuario { get; set; } = string.Empty;
            public string Contraseña { get; set; } = string.Empty;
        }

        public record CrearAdminDto(string NombreUsuario, string Contraseña);

        public async Task<List<AdminDto>> GetAdminsAsync()
            => await _http.GetFromJsonAsync<List<AdminDto>>("api/usuarios/admins") ?? new();

        public async Task<bool> CrearAsync(CrearAdminDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/usuarios/crear-admin", new
            {
                NombreUsuario = dto.NombreUsuario,
                Contraseña = dto.Contraseña
            });
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarAsync(AdminDto admin)
        {
            var resp = await _http.PutAsJsonAsync("api/usuarios/actualizar-admin", new
            {
                Id = admin.Id,
                NombreUsuario = admin.NombreUsuario,
                Contraseña = admin.Contraseña,
                Rol = "admin"
            });
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/usuarios/eliminar-admin/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}