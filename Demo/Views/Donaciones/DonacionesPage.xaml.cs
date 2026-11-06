// Lógica de Donaciones: muestra las campañas y abre el perfil de la asociación de cada una.
namespace Demo.Views.Donaciones;

// Vista previa de donaciones: no procesa pagos, solo lleva al perfil de cada asociación.
public partial class DonacionesPage : ContentPage
{
    public DonacionesPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(ListaCampanas, Datos.Campanas);
    }

    private async void OnVerAsociacionClicked(object? sender, EventArgs e)
    {
        string nombre = (sender as Button)?.CommandParameter as string ?? "";
        if (Datos.Asociaciones.FirstOrDefault(a => a.Nombre == nombre) is { } asociacion)
            await AppShell.Abrir<AsociacionDetallePage>(asociacion);
    }
}
