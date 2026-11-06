// Lógica del perfil de asociación: recibe la asociación elegida, marca favorito y abre WhatsApp.
namespace Demo.Views.Asociaciones;

// Perfil de una asociación. Se abre con AppShell.Abrir<AsociacionDetallePage>(asociacion).
public partial class AsociacionDetallePage : ContentPage, IQueryAttributable
{
    private Asociacion? asociacion;

    public AsociacionDetallePage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(AppShell.ParametroItem, out var item) && item is Asociacion a)
            BindingContext = asociacion = a;
    }

    private async void OnFavoritoTapped(object? sender, EventArgs e)
    {
        if (asociacion is not null)
            await Datos.AlternarFavorito(asociacion);
    }

    private async void OnContactarClicked(object? sender, EventArgs e)
    {
        if (asociacion is not null)
            await Contacto.WhatsApp(asociacion.Telefono);
    }
}
