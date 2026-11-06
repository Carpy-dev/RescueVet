// Lógica de Configuración: muestra la cuenta actual y oculta los enlaces de usuario cuando entra una asociación.
namespace Demo.Views.Inicio;

public partial class ConfiguracionPage : ContentPage
{
    public ConfiguracionPage()
    {
        InitializeComponent();
    }

    // En el panel de asociación es una pestaña: sin "atrás" ni enlaces a pantallas de usuario.
    // Se actualiza al aparecer porque la pestaña sigue viva al cambiar de cuenta.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        bool esUsuario = !Api.EsAsociacion;
        Cabecera.MostrarAtras = esUsuario;
        TituloCuenta.IsVisible = SeccionCuenta.IsVisible = esUsuario;
        FilaCentroAyuda.IsVisible = LineaCentroAyuda.IsVisible = esUsuario;

        var usuario = Api.UsuarioActual;
        NombreLabel.Text = usuario?.Nombre ?? "Invitado";
        CorreoLabel.Text = usuario?.Correo ?? "Sin cuenta";
        TipoLabel.Text = usuario?.Tipo switch
        {
            nameof(TipoCuenta.Asociacion) => "Asociación",
            nameof(TipoCuenta.Veterinaria) => "Veterinaria",
            _ => "Usuario común",
        };
    }

    private async void OnMiMascotaTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(MiMascotaPage));

    private async void OnCentroAyudaTapped(object? sender, TappedEventArgs e)
        => await AppShell.IrA(AppShell.RutaAyuda);

    private async void OnTerminosTapped(object? sender, TappedEventArgs e)
        => await DisplayAlertAsync("Términos y privacidad",
            "Usamos tus datos solo para conectarte con asociaciones, veterinarias y personas que necesitan ayuda. " +
            "No los compartimos con terceros. Podés pedir que borremos tu cuenta cuando quieras.", "Entendido");

    private async void OnCerrarSesionClicked(object? sender, EventArgs e)
        => await AppShell.IrA(AppShell.RutaLogin);
}
