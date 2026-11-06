// Lógica de Inicio: saluda con el nombre de la cuenta, muestra novedades y abre cada sección.
namespace Demo.Views.Inicio;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(ListaNovedades, Datos.Novedades);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Saludo.Text = Api.UsuarioActual is { } usuario ? $"¡Hola, {usuario.Nombre}! " : "¡Hola! ";
    }

    // Pantallas de detalle: se apilan sobre Inicio (con botón "‹")
    private async void OnAsociacionesTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(AsociacionesPage));

    private async void OnVeterinariasTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(VeterinariasPage));

    private async void OnMiMascotaTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(MiMascotaPage));

    private async void OnReportarTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(ReportarCasoPage));

    // Pestañas
    private async void OnDonarTapped(object? sender, TappedEventArgs e)
        => await AppShell.IrA(AppShell.RutaDonacion);

    private async void OnEmergenciaTapped(object? sender, TappedEventArgs e)
        => await AppShell.IrA(AppShell.RutaAyuda);

    private async void OnNovedadTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Novedad novedad)
            await DisplayAlertAsync(novedad.Titulo, novedad.Detalle, "OK");
    }

    private async void OnConfiguracionTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(ConfiguracionPage));
}
