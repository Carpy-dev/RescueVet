// Lógica de Reportar caso: elige la foto, la sube, guarda el caso en la API y muestra el mensaje de publicado.
namespace Demo.Views.Casos;

// Recibe ?tipo=mascota o ?tipo=animal desde la pantalla de Emergencia (solo cambia el título)
[QueryProperty(nameof(Tipo), "tipo")]
public partial class ReportarCasoPage : ContentPage
{
    private FileResult? foto;

    public ReportarCasoPage()
    {
        InitializeComponent();
        TelefonoEntry.Text = Api.UsuarioActual?.Telefono;
    }

    public string Tipo
    {
        set => Cabecera.Titulo = value switch
        {
            "mascota" => "Emergencia de mi mascota",
            "animal" => "Animal que necesita ayuda",
            _ => "Reportar caso",
        };
    }

    private async void OnSubirFotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var elegida = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elegí una foto del animal",
                FileTypes = FilePickerFileType.Images,
            });
            if (elegida is null)
                return;

            foto = elegida;
            FotoPreview.Source = ImageSource.FromStream(_ => elegida.OpenReadAsync());
            MarcoFoto.IsVisible = true;
            BotonFoto.Text = "Cambiar foto";
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Foto", "No se pudo abrir la foto.", "OK");
        }
    }

    // Guarda el caso en el backend. Las asociaciones lo ven en su pantalla de casos.
    private async void OnEnviarSolicitudClicked(object? sender, EventArgs e)
    {
        string nombre = NombreEntry.Text?.Trim() ?? "";
        string descripcion = DescripcionEditor.Text?.Trim() ?? "";
        string telefono = TelefonoEntry.Text?.Trim() ?? "";
        if (nombre.Length == 0 || descripcion.Length == 0)
        {
            await DisplayAlertAsync("Reportar caso", "Contanos qué animal es y qué le pasa.", "OK");
            return;
        }
        if (telefono.Length == 0)
        {
            await DisplayAlertAsync("Reportar caso", "Dejá tu WhatsApp para que puedan contactarte.", "OK");
            return;
        }

        // Evita publicar dos veces mientras se envía
        if (sender is Button boton)
            boton.IsEnabled = false;
        try
        {
            string rutaFoto = foto is null ? "fotos/rv_ilus_casos.png" : await Api.SubirFoto(foto);
            await Api.GuardarCaso(new Caso
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Historia = descripcion,
                Ubicacion = UbicacionEntry.Text?.Trim() ?? "",
                Especie = EspeciePicker.SelectedItem as string ?? "",
                Prioridad = PrioridadPicker.SelectedItem as string ?? "",
                Categoria = "Rescate",
                Foto = rutaFoto,
                UsuarioId = Api.UsuarioActual?.Id,
                Telefono = telefono,
            });

            await DisplayAlertAsync("¡Publicado!", "Tu caso fue publicado.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Reportar caso", ex.Message, "OK");
            if (sender is Button b)
                b.IsEnabled = true;
        }
    }
}
