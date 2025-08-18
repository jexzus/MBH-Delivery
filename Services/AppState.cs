namespace MauiBlazorDelivery.Services
{
    public class AppState
    {
        public bool IsLoggedIn { get; private set; }
        public string Rol { get; private set; } = "";
        public string NombreUsuario { get; private set; } = "";

        public event Action? OnChange;

        public void Login(string nombreUsuario, string rol)
        {
            IsLoggedIn = true;
            NombreUsuario = nombreUsuario;
            Rol = rol;
            NotifyStateChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            NombreUsuario = "";
            Rol = "";
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
