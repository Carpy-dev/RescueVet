// Lógica del detalle de caso: recibe el caso elegido y abre WhatsApp con quien lo publicó.
namespace Demo.Views.Casos;

// Detalle de un caso que necesita ayuda. Se abre con AppShell.Abrir<CasoDetallePage>(caso).
public partial class CasoDetallePage : ContentPage, IQueryAttributable
{
    private Caso? caso;

    public CasoDetallePage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(AppShell.ParametroItem, out var item) && item is Caso c)
            BindingContext = caso = c;
    }

    private async void OnContactarClicked(object? sender, EventArgs e)
    {
        if (caso is not null)
            await Contacto.WhatsApp(caso.Telefono);
    }
}
