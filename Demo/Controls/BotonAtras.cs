// Control reutilizable: botón redondo que vuelve a la pantalla anterior.
using Microsoft.Maui.Controls.Shapes;

namespace Demo.Controls;

// Botón redondo blanco con "‹" que vuelve a la pantalla anterior.
// Uso: <controls:BotonAtras />
// Si la pantalla se suscribe a Tocado, el botón no navega y deja que la pantalla decida (ej. volver un paso).
public class BotonAtras : Border
{
    public event EventHandler? Tocado;

    public BotonAtras()
    {
        var res = Application.Current!.Resources;

        WidthRequest = 46;
        HeightRequest = 46;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        BackgroundColor = (Color)res["Surface"];
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 23 };
        Shadow = new Shadow { Brush = (Color)res["TextDark"], Opacity = 0.10f, Radius = 12, Offset = new Point(0, 3) };
        Content = new Label
        {
            Text = "\uE5CB",
            FontFamily = "MaterialIcons",
            FontSize = 28,
            TextColor = (Color)res["TextDark"],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        SemanticProperties.SetDescription(this, "Volver");

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            if (Tocado is not null)
                Tocado(this, EventArgs.Empty);
            else
                await Shell.Current.GoToAsync("..");
        };
        GestureRecognizers.Add(tap);
    }
}
