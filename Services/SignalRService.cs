using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace MauiBlazorDelivery.Services;

public class SignalRService : IAsyncDisposable
{
    private readonly string _baseUrl;
    private HubConnection? _hub;

    // Eventos que las páginas suscriben
    public event Action<int, string>? EstadoCambiadoGlobal;
    public event Action<NuevoPedidoDto>? NuevoPedidoParaReparto;
    public event Action<int, string, int>? PedidoAceptado;       // numPedido, nombreRepartidor, repartidorId
    public event Action<int, int>? PedidoAsignado;               // numPedido, repartidorId  (para todos los repartidores)
    public event Action<int>? PedidoCancelado;
    public event Action<int>? CerrarAlerta;                      // numPedido → cierra modal de solicitud
    public event Action<int>? PedidoRevertido;
    public event Action<int>? AlertaAdminVista;
    public event Action<AlertaPedidoDto>? AlertaPedidoIgnorada;
    public event Action? Reconectado;

    public bool IsConnected => _hub?.State == HubConnectionState.Connected;

    public SignalRService(string baseUrl) => _baseUrl = baseUrl;

    public async Task ConnectAsync(string rol, int idUsuario, int? idCliente = null, int? idRepartidor = null, string disponibilidad = "Activo")
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }

        var url = BuildUrl(rol, idUsuario, idCliente, idRepartidor, disponibilidad);
        Console.WriteLine($"[SignalR] Connecting to {url}");

        _hub = new HubConnectionBuilder()
            .WithUrl(url, opts =>
            {
#if DEBUG
                opts.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
#endif
            })
            .WithAutomaticReconnect()
            .Build();

        _hub.Reconnected += _ => { Reconectado?.Invoke(); return Task.CompletedTask; };

        RegisterHandlers();

        try
        {
            await _hub.StartAsync();
            Console.WriteLine($"[SignalR] Connected OK. State={_hub.State}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Connect failed: {ex.Message}");
        }
    }

    private string BuildUrl(string rol, int idUsuario, int? idCliente, int? idRepartidor, string disponibilidad)
    {
        var qs = $"rol={Uri.EscapeDataString(rol)}";

        if (rol.Equals("cliente", StringComparison.OrdinalIgnoreCase))
            qs += $"&clienteId={idCliente ?? idUsuario}";

        if (rol.Equals("repartidor", StringComparison.OrdinalIgnoreCase))
            qs += $"&repartidorId={idRepartidor ?? idUsuario}&disponibilidad={disponibilidad}";

        return $"{_baseUrl.TrimEnd('/')}/deliveryHub?{qs}";
    }

    private void RegisterHandlers()
    {
        if (_hub is null) return;

        _hub.On<JsonElement>("EstadoCambiadoGlobal", el =>
        {
            var numPedido = el.GetProperty("numPedido").GetInt32();
            var nuevoEstado = el.GetProperty("nuevoEstado").GetString() ?? "";
            EstadoCambiadoGlobal?.Invoke(numPedido, nuevoEstado);
        });

        _hub.On<JsonElement>("NuevoPedidoParaReparto", el =>
        {
            var dto = new NuevoPedidoDto
            {
                NumPedido = el.GetProperty("numPedido").GetInt32(),
                Cliente = el.GetProperty("cliente").GetString() ?? "",
                Domicilio = el.GetProperty("domicilio").GetString() ?? "",
                Telefono = el.GetProperty("telefono").GetString() ?? "",
                MontoTotal = el.GetProperty("montoTotal").GetString() ?? "",
                Productos = el.GetProperty("productos").GetString() ?? "",
                Observaciones = el.GetProperty("observaciones").GetString() ?? ""
            };
            NuevoPedidoParaReparto?.Invoke(dto);
        });

        _hub.On<JsonElement>("PedidoAceptado", el =>
        {
            var numPedido = el.GetProperty("numPedido").GetInt32();
            var nombre = el.GetProperty("nombreRepartidor").GetString() ?? "";
            var repId = el.GetProperty("repartidorId").GetInt32();
            PedidoAceptado?.Invoke(numPedido, nombre, repId);
        });

        _hub.On<JsonElement>("PedidoAsignado", el =>
        {
            var numPedido = el.GetProperty("numPedido").GetInt32();
            var repId = el.GetProperty("repartidorId").GetInt32();
            PedidoAsignado?.Invoke(numPedido, repId);
        });

        _hub.On<int>("PedidoCancelado", numPedido => PedidoCancelado?.Invoke(numPedido));

        _hub.On<int>("CerrarAlerta", numPedido => CerrarAlerta?.Invoke(numPedido));

        _hub.On<JsonElement>("PedidoRevertido", el =>
        {
            var numPedido = el.GetProperty("numPedido").GetInt32();
            PedidoRevertido?.Invoke(numPedido);
        });

        _hub.On<JsonElement>("AlertaAdminVista", el =>
        {
            var numPedido = el.GetProperty("numPedido").GetInt32();
            AlertaAdminVista?.Invoke(numPedido);
        });

        _hub.On<JsonElement>("AlertaPedidoIgnorada", el =>
        {
            var dto = new AlertaPedidoDto
            {
                NumPedido = el.GetProperty("numPedido").GetInt32(),
                Mensaje = el.GetProperty("mensaje").GetString() ?? "",
                Tipo = el.GetProperty("tipo").GetString() ?? ""
            };
            AlertaPedidoIgnorada?.Invoke(dto);
        });
    }

    // ── Métodos para invocar el hub desde el cliente ──────────────────────

    public async Task RechazarPedidoAsync(int numPedido)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("RechazarPedido", numPedido);
    }

    public async Task MarcarAlertaVistaAsync(int numPedido)
    {
        if (_hub?.State == HubConnectionState.Connected)
            await _hub.InvokeAsync("MarcarAlertaVista", numPedido);
    }

    public async Task DisconnectAsync()
    {
        if (_hub is not null)
        {
            await _hub.StopAsync();
            await _hub.DisposeAsync();
            _hub = null;
        }
    }

    public async ValueTask DisposeAsync() => await DisconnectAsync();

    // ── DTOs ──────────────────────────────────────────────────────────────

    public class NuevoPedidoDto
    {
        public int NumPedido { get; set; }
        public string Cliente { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string MontoTotal { get; set; } = "";
        public string Productos { get; set; } = "";
        public string Observaciones { get; set; } = "";
    }

    public class AlertaPedidoDto
    {
        public int NumPedido { get; set; }
        public string Mensaje { get; set; } = "";
        public string Tipo { get; set; } = "";
    }
}
