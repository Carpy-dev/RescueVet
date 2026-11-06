// Lógica de Emergencia: abre Reportar caso con el tipo elegido y llama a la línea de emergencia.
namespace Demo.Views.Emergencia;

public partial class EmergenciaPage : ContentPage
{
    // TODO: reemplazar por el número real de la línea de emergencia de RescueVet
    private const string LineaEmergencia = "08000000000";

    public EmergenciaPage()
    {
        InitializeComponent();
    }

    // "mascota" o "animal": abre el formulario de reporte con ese tipo ya elegido
    private async void OnTipoTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync($"{nameof(ReportarCasoPage)}?tipo={e.Parameter}");

    private async void OnLlamarClicked(object? sender, EventArgs e)
    {
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(LineaEmergencia);
                return;
            }
        }
        catch (Exception)
        {
            // Sin marcador telefónico (por ejemplo en Windows): mostramos el número
        }
        await DisplayAlertAsync("Línea de emergencia 24/7", $"Llamá al {LineaEmergencia}.", "OK");
    }
}
