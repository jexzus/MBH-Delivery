using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class PedidosService
    {
        private readonly HttpClient _http;
        public PedidosService(HttpClient http) => _http = http;

        // ===== DTOs UI (públicos, para usar en Razor) =====
        public class PedidoDto
        {
            public int NumPedido { get; set; }
            public int IdCliente { get; set; }
            public DateTime FechaPedido { get; set; }
            public string EstadoPedido { get; set; } = "Pendiente";
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public ClienteDto? Cliente { get; set; }
            public List<DetalleDto> DetallePedidos { get; set; } = new();
        }
        public class ClienteDto
        {
            public int IdCliente { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public string? NumTelefono { get; set; }
            public string? Domicilio { get; set; }
        }
        public class DetalleDto
        {
            public int IdDetalle { get; set; }
            public int NumPedido { get; set; }
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal => Cantidad * PrecioUnitario;
            public ProductoDto? Producto { get; set; }
        }
        public class ProductoDto
        {
            public int IdProducto { get; set; }
            public string? NombreProducto { get; set; }
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public string? Imagen { get; set; }
        }

        // ===== Modelos API (privados) =====
        private class PedidoApi
        {
            public int NumPedido { get; set; }
            public int IdCliente { get; set; }
            public DateTime FechaPedido { get; set; }
            public string? EstadoPedido { get; set; }
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public ClienteApi? IdClienteNavigation { get; set; }
            public List<DetalleApi> DetallePedidos { get; set; } = new();
        }
        private class ClienteApi
        {
            public int IdCliente { get; set; }
            public string? Nombre { get; set; }
            public string? Apellido { get; set; }
            public string? NumTelefono { get; set; }
            public string? Domicilio { get; set; }
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
        private class ProductoApi
        {
            public int IdProducto { get; set; }
            public string? NombreProducto { get; set; }
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public string? Imagen { get; set; }
        }

        // ===== API calls =====
        public async Task<List<PedidoDto>> GetPedidosAsync()
        {
            var raw = await _http.GetFromJsonAsync<List<PedidoApi>>("api/admin/pedidos") ?? new();
            return Map(raw);
        }

        public async Task<bool> CambiarEstadoAsync(int numPedido, string nuevoEstado)
        {
            var resp = await _http.PutAsJsonAsync("api/admin/cambiar-estado",
                new { NumPedido = numPedido, NuevoEstado = nuevoEstado });
            return resp.IsSuccessStatusCode;
        }

        // ===== Mapper =====
        private static List<PedidoDto> Map(List<PedidoApi> raw)
            => raw.Select(r =>
            {
                var p = new PedidoDto
                {
                    NumPedido = r.NumPedido,
                    IdCliente = r.IdCliente,
                    FechaPedido = r.FechaPedido,
                    EstadoPedido = r.EstadoPedido ?? "Pendiente",
                    MontoTotal = r.MontoTotal,
                    Observaciones = r.Observaciones,
                    Cliente = r.IdClienteNavigation is null ? null : new ClienteDto
                    {
                        IdCliente = r.IdClienteNavigation.IdCliente,
                        Nombre = r.IdClienteNavigation.Nombre,
                        Apellido = r.IdClienteNavigation.Apellido,
                        NumTelefono = r.IdClienteNavigation.NumTelefono,
                        Domicilio = r.IdClienteNavigation.Domicilio
                    }
                };

                foreach (var d in r.DetallePedidos ?? new())
                {
                    p.DetallePedidos.Add(new DetalleDto
                    {
                        IdDetalle = d.IdDetalle,
                        NumPedido = d.NumPedido,
                        IdProducto = d.IdProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Producto = d.IdProductoNavigation is null ? null : new ProductoDto
                        {
                            IdProducto = d.IdProductoNavigation.IdProducto,
                            NombreProducto = d.IdProductoNavigation.NombreProducto,
                            Descripcion = d.IdProductoNavigation.Descripcion,
                            Precio = d.IdProductoNavigation.Precio,
                            Imagen = d.IdProductoNavigation.Imagen
                        }
                    });
                }
                return p;
            })
            .OrderBy(p => p.EstadoPedido == "Entregado" ? 1 : 0)
            .ThenByDescending(p => p.FechaPedido)
            .ToList();
    }
}
