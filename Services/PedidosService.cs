using System.Net.Http.Json;

namespace MauiBlazorDelivery.Services
{
    public class PedidosService
    {
        private readonly HttpClient _http;
        public PedidosService(HttpClient http) => _http = http;

        // ===== DTOs UI =====
        public class PedidoDto
        {
            public int NumPedido { get; set; }
            public int IdCliente { get; set; }
            public int? IdRepartidor { get; set; }
            public DateTime FechaPedido { get; set; }
            public string EstadoPedido { get; set; } = "Pendiente";
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public string? ModoEntrega { get; set; }
            public string? FormaPago { get; set; }
            public string EstadoPago { get; set; } = "pendiente";
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
            public int? IdRepartidor { get; set; }
            public DateTime FechaPedido { get; set; }
            public string? EstadoPedido { get; set; }
            public decimal MontoTotal { get; set; }
            public string? Observaciones { get; set; }
            public string? ModoEntrega { get; set; }
            public string? FormaPago { get; set; }
            public string? EstadoPago { get; set; }
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

        public async Task<(bool ok, string? error)> CambiarEstadoAsync(int numPedido, string nuevoEstado)
        {
            var resp = await _http.PutAsJsonAsync("api/admin/cambiar-estado",
                new { NumPedido = numPedido, NuevoEstado = nuevoEstado });
            if (resp.IsSuccessStatusCode) return (true, null);
            var msg = await resp.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(msg) ? "No se pudo actualizar el estado." : msg.Trim('"'));
        }

        public async Task<bool> SolicitarRepartidorAsync(int numPedido)
        {
            var resp = await _http.PostAsJsonAsync("api/admin/solicitar-repartidor",
                new { NumPedido = numPedido });
            return resp.IsSuccessStatusCode;
        }

        public async Task<(bool ok, string mensaje, bool yaFueTomado)> AceptarPedidoAsync(int numPedido, int idRepartidor)
        {
            var resp = await _http.PostAsJsonAsync("api/admin/aceptar-pedido",
                new { NumPedido = numPedido, IdRepartidor = idRepartidor });

            if (resp.IsSuccessStatusCode) return (true, "Pedido aceptado.", false);

            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "Este pedido ya fue tomado por otro repartidor.", true);

            return (false, "No se pudo aceptar el pedido.", false);
        }

        public async Task<List<PedidoDisponibleDto>> GetPedidosDisponiblesAsync()
        {
            return await _http.GetFromJsonAsync<List<PedidoDisponibleDto>>("api/admin/pedidos-disponibles") ?? new();
        }

        public async Task<bool> RevertirAPreparacionAsync(int numPedido, bool reemitirAlerta = true)
        {
            var resp = await _http.PostAsJsonAsync("api/admin/revertir-a-preparacion",
                new { NumPedido = numPedido, ReemitirAlerta = reemitirAlerta });
            return resp.IsSuccessStatusCode;
        }

        public async Task<(bool ok, string mensaje)> CancelarRepartoAsync(int numPedido, int idRepartidor)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/admin/cancelar-reparto",
                    new { NumPedido = numPedido, IdRepartidor = idRepartidor });

                if (resp.IsSuccessStatusCode) return (true, "");

                if ((int)resp.StatusCode == 409)
                    return (false, "Han pasado más de 5 minutos. Ya no podés cancelar este pedido.");

                return (false, "No se pudo cancelar el reparto.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ===== DTO pedidos disponibles para repartidores =====
        public class PedidoDisponibleDto
        {
            public int NumPedido { get; set; }
            public string Cliente { get; set; } = "";
            public string Domicilio { get; set; } = "";
            public string Telefono { get; set; } = "";
            public string MontoTotal { get; set; } = "";
            public string Productos { get; set; } = "";
            public string Observaciones { get; set; } = "";
        }

        // ===== Mapper =====
        private static List<PedidoDto> Map(List<PedidoApi> raw)
            => raw.Select(r =>
            {
                var p = new PedidoDto
                {
                    NumPedido = r.NumPedido,
                    IdCliente = r.IdCliente,
                    IdRepartidor = r.IdRepartidor,
                    FechaPedido = r.FechaPedido,
                    EstadoPedido = r.EstadoPedido ?? "Pendiente",
                    MontoTotal = r.MontoTotal,
                    Observaciones = r.Observaciones,
                    ModoEntrega = r.ModoEntrega,
                    FormaPago = r.FormaPago,
                    EstadoPago = r.EstadoPago ?? "pendiente",
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
