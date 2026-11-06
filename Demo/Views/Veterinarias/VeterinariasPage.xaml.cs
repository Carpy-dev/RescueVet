// Lógica de la lista de veterinarias: muestra los datos, marca favoritos y abre el detalle.
namespace Demo.Views.Veterinarias;

public partial class VeterinariasPage : ContentPage
{
    public VeterinariasPage()
    {
        InitializeComponent();
        Carrusel.ItemsSource = Datos.Veterinarias;
    }

    // Abre la primera veterinaria con atención 24/7
    private async void OnVer24Clicked(object? sender, EventArgs e)
    {
        if (Datos.Veterinarias.FirstOrDefault(v => v.Abierta24h) is { } veterinaria)
            await AppShell.Abrir<VeterinariaDetallePage>(veterinaria);
    }

    private async void OnFavoritoTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Veterinaria v)
            await Datos.AlternarFavorito(v);
    }

    private async void OnVerDetallesTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Veterinaria v)
            await AppShell.Abrir<VeterinariaDetallePage>(v);
    }
}
