// Lógica de Recordatorios: agrega, edita y elimina en la API, y actualiza la lista.
namespace Demo.Views.MiMascota;

public partial class RecordatoriosPage : ContentPage
{
    private const string Recurso = "Recordatorio";

    public RecordatoriosPage()
    {
        InitializeComponent();
        Refrescar();
    }

    private void Refrescar() => BindableLayout.SetItemsSource(Lista, Datos.MiMascota?.Recordatorios);

    private async void OnAgregarClicked(object? sender, EventArgs e)
    {
        if (Datos.MiMascota is not { } mascota)
            return;

        var titulo = new CampoForm("Título");
        var frecuencia = new CampoForm("Frecuencia", "Cada 3 meses");
        var fecha = CampoForm.DeFecha("Próxima fecha", DateOnly.FromDateTime(DateTime.Today));
        if (!await FormularioPage.Pedir("Nuevo recordatorio", titulo, frecuencia, fecha))
            return;

        var datos = new { mascotaId = mascota.Id, titulo = titulo.Valor, frecuencia = frecuencia.Valor, fecha = fecha.Fecha };
        if (await Datos.CambiarMascota(() => Api.Guardar(Recurso, datos)))
            Refrescar();
    }

    private async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Recordatorio item)
            return;

        switch (await FormularioPage.PedirAccion(item.Titulo))
        {
            case "Editar":
                var titulo = new CampoForm("Título", item.Titulo);
                var frecuencia = new CampoForm("Frecuencia", item.Frecuencia);
                var fecha = CampoForm.DeFecha("Próxima fecha", item.Fecha);
                if (!await FormularioPage.Pedir("Editar recordatorio", titulo, frecuencia, fecha))
                    return;

                var editado = item with { Titulo = titulo.Valor, Frecuencia = frecuencia.Valor, Fecha = fecha.Fecha!.Value };
                if (await Datos.CambiarMascota(() => Api.Editar(Recurso, item.Id, editado)))
                    Refrescar();
                break;

            case "Eliminar":
                if (await Datos.CambiarMascota(() => Api.Eliminar(Recurso, item.Id)))
                    Refrescar();
                break;
        }
    }
}
