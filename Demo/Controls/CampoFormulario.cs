// Control reutilizable: campo de formulario con etiqueta, ícono, ayuda y opción de mostrar la contraseña. Texto devuelve lo escrito.
using Microsoft.Maui.Controls.Shapes;

namespace Demo.Controls;

// Campo de formulario completo:
//   Etiqueta
//   [ícono  texto ...................  👁]
//   Mensaje de ayuda (gris)
// Uso: <controls:CampoFormulario x:Name="Correo" Etiqueta="Correo" Glifo="&#xE0BE;" Placeholder="tu@correo.com" Teclado="Email" />
public class CampoFormulario : ContentView
{
    public static readonly BindableProperty EtiquetaProperty =
        BindableProperty.Create(nameof(Etiqueta), typeof(string), typeof(CampoFormulario), "",
            propertyChanged: (b, _, n) => ((CampoFormulario)b).etiqueta.Text = (string)n);
    public static readonly BindableProperty GlifoProperty =
        BindableProperty.Create(nameof(Glifo), typeof(string), typeof(CampoFormulario), "",
            propertyChanged: (b, _, n) => ((CampoFormulario)b).ActualizarIcono());
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CampoFormulario), "",
            propertyChanged: (b, _, n) => ((CampoFormulario)b).entrada.Placeholder = (string)n);
    public static readonly BindableProperty TecladoProperty =
        BindableProperty.Create(nameof(Teclado), typeof(Keyboard), typeof(CampoFormulario), Keyboard.Default,
            propertyChanged: (b, _, n) => ((CampoFormulario)b).entrada.Keyboard = (Keyboard)n);
    public static readonly BindableProperty EsContraseñaProperty =
        BindableProperty.Create(nameof(EsContraseña), typeof(bool), typeof(CampoFormulario), false,
            propertyChanged: (b, _, n) => ((CampoFormulario)b).ConfigurarContraseña((bool)n));
    public static readonly BindableProperty AyudaProperty =
        BindableProperty.Create(nameof(Ayuda), typeof(string), typeof(CampoFormulario), "",
            propertyChanged: (b, _, n) => ((CampoFormulario)b).ActualizarMensaje());
    public static readonly BindableProperty ColorFocoProperty =
        BindableProperty.Create(nameof(ColorFoco), typeof(Color), typeof(CampoFormulario), null);

    public string Etiqueta { get => (string)GetValue(EtiquetaProperty); set => SetValue(EtiquetaProperty, value); }
    public string Glifo { get => (string)GetValue(GlifoProperty); set => SetValue(GlifoProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public Keyboard Teclado { get => (Keyboard)GetValue(TecladoProperty); set => SetValue(TecladoProperty, value); }
    public bool EsContraseña { get => (bool)GetValue(EsContraseñaProperty); set => SetValue(EsContraseñaProperty, value); }
    // Texto gris de ayuda debajo del campo
    public string Ayuda { get => (string)GetValue(AyudaProperty); set => SetValue(AyudaProperty, value); }
    public Color? ColorFoco { get => (Color?)GetValue(ColorFocoProperty); set => SetValue(ColorFocoProperty, value); }

    // Lo que escribió el usuario
    public string Texto
    {
        get => entrada.Text?.Trim() ?? "";
        set => entrada.Text = value;
    }

    private readonly Label etiqueta, icono, ojo, mensaje;
    private readonly Entry entrada;
    private readonly Border caja;

    public CampoFormulario()
    {
        var res = Application.Current!.Resources;

        etiqueta = new Label { FontFamily = "NunitoBold", FontSize = 14, TextColor = (Color)res["TextDark"] };
        icono = new Label { Style = (Style)res["IconoCampo"], IsVisible = false };
        entrada = new Entry { FontSize = 16 };
        ojo = new Label { Style = (Style)res["IconoCampo"], Text = "\uE8F4", Margin = new Thickness(8, 0, 0, 0), IsVisible = false };
        mensaje = new Label { FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0) };

        var tapOjo = new TapGestureRecognizer();
        tapOjo.Tapped += (_, _) =>
        {
            entrada.IsPassword = !entrada.IsPassword;
            ojo.Text = entrada.IsPassword ? "\uE8F4" : "\uE8F5";
        };
        ojo.GestureRecognizers.Add(tapOjo);

        entrada.Focused += (_, _) => PintarBorde(enfocado: true);
        entrada.Unfocused += (_, _) => PintarBorde(enfocado: false);

        var fila = new Grid
        {
            ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
        };
        fila.Add(icono, 0);
        fila.Add(entrada, 1);
        fila.Add(ojo, 2);

        caja = new Border
        {
            BackgroundColor = (Color)res["Surface"],
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(16, 2),
            MinimumHeightRequest = 54,
            Content = fila,
        };

        var raiz = new VerticalStackLayout { Spacing = 6 };
        raiz.Add(etiqueta);
        raiz.Add(caja);
        raiz.Add(mensaje);
        Content = raiz;

        PintarBorde(enfocado: false);
    }

    private void ActualizarIcono()
    {
        icono.Text = Glifo;
        icono.IsVisible = !string.IsNullOrEmpty(Glifo);
    }

    private void ConfigurarContraseña(bool esContraseña)
    {
        entrada.IsPassword = esContraseña;
        ojo.IsVisible = esContraseña;
    }

    private void ActualizarMensaje()
    {
        mensaje.Text = Ayuda;
        mensaje.TextColor = (Color)Application.Current!.Resources["Muted"];
        mensaje.IsVisible = !string.IsNullOrEmpty(mensaje.Text);
    }

    private void PintarBorde(bool enfocado)
    {
        var res = Application.Current!.Resources;
        caja.Stroke = enfocado ? ColorFoco ?? (Color)res["Primary"] : Color.FromArgb("#DCE6EE");
        icono.TextColor = enfocado ? ColorFoco ?? (Color)res["Primary"] : (Color)res["Muted"];
    }
}
