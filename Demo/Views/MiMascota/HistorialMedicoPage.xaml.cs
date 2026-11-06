namespace Demo.Views.MiMascota;

// Solo lectura: el usuario ve el historial de su mascota, pero no lo modifica.
public partial class HistorialMedicoPage : ContentPage
{
    public HistorialMedicoPage()
    {
        InitializeComponent();
        BindingContext = Datos.MiMascota;
        BindableLayout.SetItemsSource(Registros, Datos.MiMascota?.Historial);
    }
}
