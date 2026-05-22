namespace MauiBlazorDelivery.Services
{
    public class AppState
    {
        public bool IsLoggedIn { get; private set; }
        public string Rol { get; private set; } = "";
        public string NombreUsuario { get; private set; } = "";
        public int IdUsuario { get; private set; }
        public int? IdCliente { get; private set; }
        public int? IdRepartidor { get; private set; }

        public event Action? OnChange;

        public void Login(string nombreUsuario, string rol, int idUsuario, int? idCliente = null, int? idRepartidor = null)
        {
            IsLoggedIn = true;
            NombreUsuario = nombreUsuario;
            Rol = rol;
            IdUsuario = idUsuario;
            IdCliente = idCliente;
            IdRepartidor = idRepartidor;
            NotifyStateChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            NombreUsuario = "";
            Rol = "";
            IdUsuario = 0;
            IdCliente = null;
            IdRepartidor = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
