// Acciones de contacto de las pantallas de detalle: abrir un chat de WhatsApp y abrir la ruta en el mapa.
namespace Demo.Services;

// Acciones de contacto que usan las pantallas de detalle.
public static class Contacto
{
    // Código de Argentina para celulares: se agrega a los números de 10 dígitos (ej. 3704893267)
    private const string CodigoPais = "549";

    // Abre un chat de WhatsApp con ese número
    public static async Task WhatsApp(string numero)
    {
        string digitos = new(numero.Where(char.IsDigit).ToArray());
        if (digitos.Length == 0)
        {
            await Shell.Current.DisplayAlertAsync("WhatsApp", "No hay un número de contacto cargado.", "OK");
            return;
        }

        string destino = digitos.Length == 10 ? CodigoPais + digitos : digitos;
        string mensaje = Uri.EscapeDataString("Hola, te escribo desde RescueVet.");
        try
        {
            await Launcher.Default.OpenAsync(new Uri($"https://wa.me/{destino}?text={mensaje}"));
        }
        catch (Exception)
        {
            // Sin navegador ni WhatsApp: mostramos el número
            await Shell.Current.DisplayAlertAsync("WhatsApp", numero, "OK");
        }
    }

    public static async Task ComoLlegar(string nombre, string direccion)
    {
        try
        {
            await Map.Default.OpenAsync(new Placemark { Thoroughfare = direccion, Locality = "" },
                new MapLaunchOptions { Name = nombre, NavigationMode = NavigationMode.Driving });
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlertAsync(nombre, direccion, "OK");
        }
    }
}
