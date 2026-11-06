// Formulario reutilizable de Mi Mascota, armado desde código. Se abre con Pedir y devuelve true al guardar.
// CampoForm describe cada dato, y PedirAccion muestra el menú Editar o Eliminar.
namespace Demo.Views.MiMascota;

// Un dato del formulario. Si tiene Fecha se muestra un selector de fecha; si no, un campo de texto.
public class CampoForm(string etiqueta, string valor = "", Keyboard? teclado = null)
{
    public string Etiqueta { get; } = etiqueta;
    public string Valor { get; set; } = valor;
    public Keyboard Teclado { get; } = teclado ?? Keyboard.Default;
    public DateOnly? Fecha { get; set; }

    public static CampoForm DeFecha(string etiqueta, DateOnly fecha) => new(etiqueta) { Fecha = fecha };
}

// Formulario simple que usan las pantallas de Mi Mascota:
//   if (await FormularioPage.Pedir("Nuevo peso", fecha, kg)) { ... usar fecha.Fecha y kg.Valor ... }
public class FormularioPage : ContentPage
{
    private readonly TaskCompletionSource<bool> resultado = new();

    private FormularioPage(string titulo, CampoForm[] campos)
    {
        Title = titulo;
        Shell.SetNavBarIsVisible(this, false);

        var lista = new VerticalStackLayout { Padding = new Thickness(20, 24), Spacing = 16 };
        lista.Add(new Label { Text = titulo, Style = (Style)Application.Current!.Resources["PageTitle"], FontSize = 26 });

        var lecturas = new List<Action>();
        foreach (var campo in campos)
        {
            if (campo.Fecha is { } fecha)
            {
                var selector = new DatePicker { Date = fecha.ToDateTime(TimeOnly.MinValue) };
                lista.Add(new Label { Text = campo.Etiqueta, FontFamily = "NunitoBold", FontSize = 14 });
                lista.Add(selector);
                lecturas.Add(() => campo.Fecha = DateOnly.FromDateTime(selector.Date ?? DateTime.Today));
            }
            else
            {
                var entrada = new Controls.CampoFormulario { Etiqueta = campo.Etiqueta, Teclado = campo.Teclado, Texto = campo.Valor };
                lista.Add(entrada);
                lecturas.Add(() => campo.Valor = entrada.Texto);
            }
        }

        var guardar = new Button { Text = "Guardar", Style = (Style)Application.Current.Resources["PrimaryButton"] };
        guardar.Clicked += async (_, _) =>
        {
            lecturas.ForEach(leer => leer());
            resultado.TrySetResult(true);
            await Navigation.PopModalAsync();
        };

        var cancelar = new Button { Text = "Cancelar", BackgroundColor = Colors.Transparent, TextColor = (Color)Application.Current.Resources["Primary"] };
        cancelar.Clicked += async (_, _) => await Navigation.PopModalAsync();

        lista.Add(guardar);
        lista.Add(cancelar);

        BackgroundColor = (Color)Application.Current.Resources["Bg"];
        Content = new ScrollView { Content = lista };
    }

    // Si se cierra sin guardar (botón atrás de Android) cuenta como cancelar
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        resultado.TrySetResult(false);
    }

    public static async Task<bool> Pedir(string titulo, params CampoForm[] campos)
    {
        var pagina = new FormularioPage(titulo, campos);
        await Shell.Current.Navigation.PushModalAsync(pagina);
        return await pagina.resultado.Task;
    }

    // Menú común al tocar un elemento: devuelve "Editar", "Eliminar" o null
    public static async Task<string?> PedirAccion(string titulo)
    {
        string accion = await Shell.Current.DisplayActionSheetAsync(titulo, "Cancelar", null, "Editar", "Eliminar");
        return accion is "Editar" or "Eliminar" ? accion : null;
    }
}
