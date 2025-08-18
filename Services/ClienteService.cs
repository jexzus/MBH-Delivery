using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class ClienteService
    {
        private readonly HttpClient _http;
        public ClienteService(HttpClient http) => _http = http;

        // ---- Helper para armar URL completa de imágenes ----
        private static string? BuildImageUrl(HttpClient http, string? img)
        {
            if (string.IsNullOrWhiteSpace(img)) return null;
            var baseAddr = http.BaseAddress ?? new Uri("http://localhost/");
            return new Uri(baseAddr, $"imagenes/{img}").ToString();
        }

        // ==================== DTOs expuestos a la UI ====================
        public class ProductoDto
        {
            public int IdProducto { get; set; }
            public string? NombreProducto { get; set; }
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public string? ImagenUrl { get; set; }   // <- usado por Catalogo.razor
        }

        public class CarritoItemDto
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal => Cantidad * PrecioUnitario;
            public ProductoDto? Producto { get; set; }
        }

        public class PedidoDto
        {
            public int NumPedido { get; set; }
            public DateTime FechaPedido { get; set; }
            public string EstadoPedido { get; set; } = "Pendiente";
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public List<CarritoItemDto> DetallePedidos { get; set; } = new();
        }

        // ==================== Modelos privados (API) ====================
        private class ProductoApi
        {
            public int IdProducto { get; set; }
            public string? NombreProducto { get; set; }
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public string? Imagen { get; set; }
        }

        private class DetalleApi
        {
            public int IdDetalle { get; set; }
            public int NumPedido { get; set; }
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public ProductoApi? IdProductoNavigation { get; set; }
        }

        private class PedidoApi
        {
            public int NumPedido { get; set; }
            public DateTime FechaPedido { get; set; }
            public string? EstadoPedido { get; set; }
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public List<DetalleApi> DetallePedidos { get; set; } = new();
        }

        // ==================== Llamados ====================

        public async Task<List<ProductoDto>> GetCatalogoAsync()
        {
            try
            {
                var raw = await _http.GetFromJsonAsync<List<ProductoApi>>("api/cliente/catalogo") ?? new();
                return raw.Select(p => new ProductoDto
                {
                    IdProducto = p.IdProducto,
                    NombreProducto = p.NombreProducto,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    ImagenUrl = BuildImageUrl(_http, p.Imagen)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] GetCatalogoAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<List<CarritoItemDto>> GetCarritoAsync(int idUsuario)
        {
            try
            {
                var raw = await _http.GetFromJsonAsync<List<DetalleApi>>($"api/cliente/carrito?idUsuario={idUsuario}") ?? new();
                return raw.Select(d => new CarritoItemDto
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Producto = d.IdProductoNavigation is null ? null : new ProductoDto
                    {
                        IdProducto = d.IdProductoNavigation.IdProducto,
                        NombreProducto = d.IdProductoNavigation.NombreProducto,
                        Descripcion = d.IdProductoNavigation.Descripcion,
                        Precio = d.IdProductoNavigation.Precio,
                        ImagenUrl = BuildImageUrl(_http, d.IdProductoNavigation.Imagen)
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] GetCarritoAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> AgregarAlCarritoAsync(int idUsuario, int idProducto, int cantidad)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/cliente/carrito/agregar", new
                {
                    IdUsuario = idUsuario,
                    IdProducto = idProducto,
                    Cantidad = cantidad
                });
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] AgregarAlCarritoAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarDelCarritoAsync(int idUsuario, int idProducto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/cliente/carrito/eliminar", new
                {
                    IdUsuario = idUsuario,
                    IdProducto = idProducto
                });
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] EliminarDelCarritoAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConfirmarPedidoAsync(int idUsuario, string observaciones)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/cliente/confirmar-pedido", new
                {
                    IdUsuario = idUsuario,
                    Observaciones = observaciones
                });
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] ConfirmarPedidoAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PedidoDto>> GetMisPedidosAsync(int idUsuario)
        {
            try
            {
                var resp = await _http.GetAsync($"api/cliente/estado-pedido/{idUsuario}");
                if (!resp.IsSuccessStatusCode) return new();

                var raw = await resp.Content.ReadFromJsonAsync<List<PedidoApi>>() ?? new();

                return raw.Select(p => new PedidoDto
                {
                    NumPedido = p.NumPedido,
                    FechaPedido = p.FechaPedido,
                    EstadoPedido = p.EstadoPedido ?? "Pendiente",
                    MontoTotal = p.MontoTotal,
                    Observaciones = p.Observaciones,
                    DetallePedidos = p.DetallePedidos.Select(d => new CarritoItemDto
                    {
                        IdProducto = d.IdProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Producto = d.IdProductoNavigation is null ? null : new ProductoDto
                        {
                            IdProducto = d.IdProductoNavigation.IdProducto,
                            NombreProducto = d.IdProductoNavigation.NombreProducto,
                            Descripcion = d.IdProductoNavigation.Descripcion,
                            Precio = d.IdProductoNavigation.Precio,
                            ImagenUrl = BuildImageUrl(_http, d.IdProductoNavigation.Imagen)
                        }
                    }).ToList()
                })
                // primero no entregados; dentro de cada grupo, más recientes primero
                .OrderBy(p => p.EstadoPedido.Equals("Entregado", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenByDescending(p => p.FechaPedido)
                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClienteService] GetMisPedidosAsync error: {ex.Message}");
                return new();
            }
        }
    }
}
