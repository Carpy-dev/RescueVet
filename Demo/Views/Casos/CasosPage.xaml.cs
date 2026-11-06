// Lógica de la lista de casos: muestra los casos cargados y abre el detalle del elegido.
namespace Demo.Views.Casos;

public partial class CasosPage : ContentPage
{
    public CasosPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(ListaCasos, Datos.Casos);
    }

    // Foto, descripción o botón "Ver caso": abren el detalle
    private async void OnCasoTapped(object? sender, TappedEventArgs e) => await AbrirDetalle(sender);

    private async void OnVerCasoClicked(object? sender, EventArgs e) => await AbrirDetalle(sender);

    private static async Task AbrirDetalle(object? sender)
    {
        if ((sender as BindableObject)?.BindingContext is Caso caso)
            await AppShell.Abrir<CasoDetallePage>(caso);
    }
}
