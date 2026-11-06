// Lógica de Crecimiento: registra, edita y elimina pesos en la API. Acepta 27,5 y 27.5.
using System.Globalization;

namespace Demo.Views.MiMascota;

public partial class CrecimientoPage : ContentPage
{
    private const string Recurso = "Peso";

    public CrecimientoPage()
    {
        InitializeComponent();
        Refrescar();
    }

    private void Refrescar()
    {
        BindingContext = Datos.MiMascota;
        BindableLayout.SetItemsSource(Barras, Datos.MiMascota?.Pesos);
    }

    // Acepta tanto "27,5" como "27.5"
    private static double LeerKg(string texto)
        => double.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double kg) ? kg : 0;

    private async void OnAgregarClicked(object? sender, EventArgs e)
    {
        if (Datos.MiMascota is not { } mascota)
            return;

        var fecha = CampoForm.DeFecha("Fecha", DateOnly.FromDateTime(DateTime.Today));
        var kg = new CampoForm("Peso en kg", teclado: Keyboard.Numeric);
        if (!await FormularioPage.Pedir("Registrar peso", fecha, kg))
            return;

        var datos = new { mascotaId = mascota.Id, fecha = fecha.Fecha, kg = LeerKg(kg.Valor) };
        if (await Datos.CambiarMascota(() => Api.Guardar(Recurso, datos)))
            Refrescar();
    }

    private async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Peso item)
            return;

        switch (await FormularioPage.PedirAccion($"{item.FechaTexto} · {item.KgTexto}"))
        {
            case "Editar":
                var fecha = CampoForm.DeFecha("Fecha", item.Fecha);
                var kg = new CampoForm("Peso en kg", item.Kg.ToString(CultureInfo.InvariantCulture), Keyboard.Numeric);
                if (!await FormularioPage.Pedir("Editar peso", fecha, kg))
                    return;

                var editado = item with { Fecha = fecha.Fecha!.Value, Kg = LeerKg(kg.Valor) };
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
