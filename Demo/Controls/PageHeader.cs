// Control reutilizable: cabecera de pantalla con botón atrás, título, subtítulo e ilustración.
namespace Demo.Controls;

// Cabecera común de las pantallas (como en los diseños):
//   [‹]
//   Título 🐾                    [ilustración]
//   Subtítulo
// Uso en XAML:
//   <controls:PageHeader Titulo="Veterinarias" Subtitulo="..." Imagen="rv_ilus_vet.png" />
public class PageHeader : ContentView
{
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(PageHeader), "", propertyChanged: Rehacer);
    public static readonly BindableProperty SubtituloProperty =
        BindableProperty.Create(nameof(Subtitulo), typeof(string), typeof(PageHeader), "", propertyChanged: Rehacer);
    public static readonly BindableProperty ImagenProperty =
        BindableProperty.Create(nameof(Imagen), typeof(string), typeof(PageHeader), "", propertyChanged: Rehacer);
    public static readonly BindableProperty MostrarAtrasProperty =
        BindableProperty.Create(nameof(MostrarAtras), typeof(bool), typeof(PageHeader), true, propertyChanged: Rehacer);
    public static readonly BindableProperty InsigniaProperty =
        BindableProperty.Create(nameof(Insignia), typeof(string), typeof(PageHeader), "pata", propertyChanged: Rehacer);
    public static readonly BindableProperty AnchoImagenProperty =
        BindableProperty.Create(nameof(AnchoImagen), typeof(double), typeof(PageHeader), 140.0, propertyChanged: Rehacer);

    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Subtitulo { get => (string)GetValue(SubtituloProperty); set => SetValue(SubtituloProperty, value); }
    public string Imagen { get => (string)GetValue(ImagenProperty); set => SetValue(ImagenProperty, value); }
    public bool MostrarAtras { get => (bool)GetValue(MostrarAtrasProperty); set => SetValue(MostrarAtrasProperty, value); }
    // "pata" (🐾 violeta), "sos" (insignia roja) o "" (nada)
    public string Insignia { get => (string)GetValue(InsigniaProperty); set => SetValue(InsigniaProperty, value); }
    public double AnchoImagen { get => (double)GetValue(AnchoImagenProperty); set => SetValue(AnchoImagenProperty, value); }

    public PageHeader() => Construir();

    private static void Rehacer(BindableObject b, object viejo, object nuevo) => ((PageHeader)b).Construir();

    private static Color Recurso(string clave) => (Color)Application.Current!.Resources[clave];

    private void Construir()
    {
        var titulo = new Label { Style = (Style)Application.Current!.Resources["PageTitle"] };
        var texto = new FormattedString();
        // En Windows los Span no heredan la fuente del Label: se la ponemos explícita
        texto.Spans.Add(new Span { Text = Insignia is "pata" or "sos" ? Titulo + " " : Titulo, FontFamily = "NunitoExtraBold" });
        if (Insignia == "pata")
            texto.Spans.Add(new Span { Text = "\uE91D", FontFamily = "MaterialIcons", FontSize = 24, TextColor = Recurso("Primary") });
        else if (Insignia == "sos")
            texto.Spans.Add(new Span { Text = "\uEBF7", FontFamily = "MaterialIcons", FontSize = 30, TextColor = Recurso("Red") });
        titulo.FormattedText = texto;

        var textos = new VerticalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
        textos.Add(titulo);
        if (!string.IsNullOrEmpty(Subtitulo))
            textos.Add(new Label { Text = Subtitulo, Style = (Style)Application.Current!.Resources["PageSubtitle"] });

        var fila = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8,
        };
        fila.Add(textos, 0);
        if (!string.IsNullOrEmpty(Imagen))
            fila.Add(new Image { Source = Imagen, WidthRequest = AnchoImagen, Aspect = Aspect.AspectFit, VerticalOptions = LayoutOptions.Center }, 1);

        var raiz = new VerticalStackLayout { Spacing = 4 };
        if (MostrarAtras)
            raiz.Add(new BotonAtras());
        raiz.Add(fila);
        Content = raiz;
    }
}
