// Lógica de la lista de asociaciones: muestra los datos del backend, marca favoritos y abre el perfil.
namespace Demo.Views.Asociaciones;

public partial class AsociacionesPage : ContentPage
{
    public AsociacionesPage()
    {
        InitializeComponent();
        Carrusel.ItemsSource = Datos.Asociaciones;
    }

    private async void OnFavoritoTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Asociacion a)
            await Datos.AlternarFavorito(a);
    }

    private async void OnVerPerfilTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Asociacion a)
            await AppShell.Abrir<AsociacionDetallePage>(a);
    }

    private async void OnDonarClicked(object? sender, EventArgs e) => await AppShell.IrA(AppShell.RutaDonacion);
}
