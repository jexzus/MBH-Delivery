using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


namespace MauiBlazorDelivery.Services
{
    public class ProductoService
    {
        private readonly HttpClient _httpClient;
        private readonly ImagenCacheService _imagenCache;

        public ProductoService(HttpClient httpClient, ImagenCacheService imagenCache)
        {
            _httpClient = httpClient;
            _imagenCache = imagenCache;
        }

        public async Task<List<ProductoDto>> GetTodosAsync()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoFromApi>>("api/Producto")
                               ?? new List<ProductoFromApi>();

                var tasks = productos.Select(async p => new ProductoDto
                {
                    Id = p.IdProducto,
                    Nombre = p.NombreProducto,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio ?? 0,
                    ImagenUrl = await _imagenCache.GetDataUrlAsync(p.Imagen)
                });
                return (await Task.WhenAll(tasks)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GET productos: {ex.Message}");
                return new List<ProductoDto>();
            }
        }

        public async Task<ProductoDto?> GetPorIdAsync(int id)
        {
            try
            {
                var p = await _httpClient.GetFromJsonAsync<ProductoFromApi>($"api/Producto/{id}");
                if (p == null) return null;
                return new ProductoDto
                {
                    Id = p.IdProducto,
                    Nombre = p.NombreProducto,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio ?? 0,
                    ImagenUrl = await _imagenCache.GetDataUrlAsync(p.Imagen)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GET producto {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CrearAsync(ProductoCreateDto dto)
        {
            try
            {
                var body = new ProductoFromApi
                {
                    NombreProducto = dto.Nombre ?? "",
                    Descripcion = dto.Descripcion ?? "",
                    Precio = dto.Precio
                };

                var resp = await _httpClient.PostAsJsonAsync("api/Producto", body);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"POST producto: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(int id, ProductoCreateDto dto)
        {
            try
            {
                var body = new ProductoFromApi
                {
                    IdProducto = id,
                    NombreProducto = dto.Nombre ?? "",
                    Descripcion = dto.Descripcion ?? "",
                    Precio = dto.Precio
                };

                var resp = await _httpClient.PutAsJsonAsync($"api/Producto/{id}", body);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PUT producto {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                var resp = await _httpClient.DeleteAsync($"api/Producto/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DELETE producto {id}: {ex.Message}");
                return false;
            }
        }
    }

    // DTOs 
}

// DTOs para tu aplicación MAUI
public class ProductoDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
    }

    public class ProductoCreateDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        // Para imágenes se necesita manejar multipart/form-data por separado
    }

    // Modelo que coincide con tu API (para deserialización)
    public class ProductoFromApi
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? Imagen { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
    }
