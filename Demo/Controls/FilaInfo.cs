// Control reutilizable: fila con ícono, título y valor, usada para dirección, horario y teléfono.
using Microsoft.Maui.Controls.Shapes;

namespace Demo.Controls;

// Fila de información: [ícono]  Título
//                               Valor
// Uso: <controls:FilaInfo Glifo="&#xE55F;" Titulo="Dirección" Valor="{Binding Direccion}" />
public class FilaInfo : ContentView
{
    public static readonly BindableProperty GlifoProperty =
        BindableProperty.Create(nameof(Glifo), typeof(string), typeof(FilaInfo), "",
            propertyChanged: (b, _, n) => ((FilaInfo)b).icono.Text = (string)n);
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(FilaInfo), "",
            propertyChanged: (b, _, n) => ((FilaInfo)b).titulo.Text = (string)n);
    public static readonly BindableProperty ValorProperty =
        BindableProperty.Create(nameof(Valor), typeof(string), typeof(FilaInfo), "",
            propertyChanged: (b, _, n) => ((FilaInfo)b).valor.Text = (string)n);
    public static readonly BindableProperty ColorProperty =
        BindableProperty.Create(nameof(Color), typeof(Color), typeof(FilaInfo), null,
            propertyChanged: (b, _, n) => ((FilaInfo)b).Pintar((Color)n));

    public string Glifo { get => (string)GetValue(GlifoProperty); set => SetValue(GlifoProperty, value); }
    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Valor { get => (string)GetValue(ValorProperty); set => SetValue(ValorProperty, value); }
    public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }

    private readonly Label icono, titulo, valor;
    private readonly Border caja;

    public FilaInfo()
    {
        var res = Application.Current!.Resources;

        icono = new Label { Style = (Style)res["Icono"], FontSize = 22 };
        caja = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            VerticalOptions = LayoutOptions.Center,
            Content = icono,
        };
        titulo = new Label { Style = (Style)res["SmallText"], FontSize = 12 };
        valor = new Label { FontFamily = "NunitoBold", FontSize = 15 };

        var textos = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        textos.Add(titulo);
        textos.Add(valor);

        var grid = new Grid { ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star) }, ColumnSpacing = 14 };
        grid.Add(caja, 0);
        grid.Add(textos, 1);
        Content = grid;

        Pintar((Color)res["Primary"]);
    }

    private void Pintar(Color color)
    {
        icono.TextColor = color;
        caja.BackgroundColor = color.WithAlpha(0.12f);
    }
}
