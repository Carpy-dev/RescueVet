// Lógica del login: valida la cuenta con la API, carga los datos y entra a las pestañas según el tipo de cuenta.
namespace Demo.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        try
        {
            Api.UsuarioActual = await Api.Login(EmailEntry.Text ?? "", PasswordEntry.Text ?? "");
            await Entrar();
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Iniciar sesión", ex.Message, "OK");
        }
    }

    // Facebook y Google siguen siendo de muestra: entran sin cuenta
    private async void OnRedSocialClicked(object? sender, EventArgs e)
    {
        try
        {
            Api.UsuarioActual = null;
            await Entrar();
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Iniciar sesión", ex.Message, "OK");
        }
    }

    // Carga los datos del backend. La ruta absoluta reemplaza el login por las pestañas de cada tipo de cuenta.
    private async Task Entrar()
    {
        await Datos.CargarAsync();
        PasswordEntry.Text = "";
        await AppShell.IrA(Api.EsAsociacion ? AppShell.RutaPanel : AppShell.RutaInicio);
    }

    private async void OnCrearCuentaTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(RegistroPage));
}
