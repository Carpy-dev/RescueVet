// Lógica del perfil de veterinaria: recibe la veterinaria elegida, marca favorito, abre WhatsApp y el mapa.
namespace Demo.Views.Veterinarias;

// Perfil de una veterinaria. Se abre con AppShell.Abrir<VeterinariaDetallePage>(veterinaria).
public partial class VeterinariaDetallePage : ContentPage, IQueryAttributable
{
    private Veterinaria? veterinaria;

    public VeterinariaDetallePage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(AppShell.ParametroItem, out var item) && item is Veterinaria v)
            BindingContext = veterinaria = v;
    }

    private async void OnFavoritoTapped(object? sender, EventArgs e)
    {
        if (veterinaria is not null)
            await Datos.AlternarFavorito(veterinaria);
    }

    private async void OnContactarClicked(object? sender, EventArgs e)
    {
        if (veterinaria is not null)
            await Contacto.WhatsApp(veterinaria.Telefono);
    }

    private async void OnComoLlegarClicked(object? sender, EventArgs e)
    {
        if (veterinaria is not null)
            await Contacto.ComoLlegar(veterinaria.Nombre, veterinaria.Direccion);
    }
}
