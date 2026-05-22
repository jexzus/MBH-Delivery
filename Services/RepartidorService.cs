using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class RepartidorService
    {
        private readonly HttpClient _http;
        public RepartidorService(HttpClient http) => _http = http;

        public class RepartidorDto
        {
            public int Id { get; set; }
            public int IdUsuario { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public bool Activo { get; set; }
            public DateTime FechaAlta { get; set; }
            public string Disponibilidad { get; set; } = "";
            public string? NombreUsuario { get; set; }
        }

        public class RepartidorRequest
        {
            public int IdUsuario { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public bool Activo { get; set; } = true;
            public string? Disponibilidad { get; set; }
        }

        public async Task<List<RepartidorDto>> GetTodosAsync()
            => await _http.GetFromJsonAsync<List<RepartidorDto>>("api/repartidores") ?? new();

        public async Task<List<RepartidorDto>> GetDisponiblesAsync()
            => await _http.GetFromJsonAsync<List<RepartidorDto>>("api/repartidores/disponibles") ?? new();

        public async Task<bool> CrearAsync(RepartidorRequest req)
        {
            var resp = await _http.PostAsJsonAsync("api/repartidores", req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarAsync(int id, RepartidorRequest req)
        {
            var resp = await _http.PutAsJsonAsync($"api/repartidores/{id}", req);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarDisponibilidadAsync(int id, string disponibilidad)
        {
            var resp = await _http.PutAsJsonAsync($"api/repartidores/{id}/disponibilidad",
                new { Disponibilidad = disponibilidad });
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/repartidores/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}
