// Lógica de Favoritos: muestra la lista, quita elementos y abre el detalle según el tipo.
namespace Demo.Views.Favoritos;

public partial class FavoritosPage : ContentPage
{
    public FavoritosPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(Lista, Datos.Favoritos);
    }

    private async void OnFavoritoTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is ItemFavoritable item)
            await Datos.AlternarFavorito(item);
    }

    // Abre el detalle según el tipo de favorito
    private async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        switch ((sender as BindableObject)?.BindingContext)
        {
            case Asociacion a: await AppShell.Abrir<AsociacionDetallePage>(a); break;
            case Veterinaria v: await AppShell.Abrir<VeterinariaDetallePage>(v); break;
            case Caso c: await AppShell.Abrir<CasoDetallePage>(c); break;
        }
    }
}
