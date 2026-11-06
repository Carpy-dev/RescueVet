// Cliente del backend: todas las llamadas a la API pasan por aquí, y también guarda quién inició sesión.
// Convierte los errores en ApiException para mostrarlos. Si cambia el Wi-Fi, se actualiza la constante IpPc.
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Demo.Services;

// Error para mostrarle al usuario (el servidor lo rechazó o no se pudo conectar)
public class ApiException(string mensaje) : Exception(mensaje);

// Cliente del backend (carpeta Backend). Todas las rutas responden { mensaje, response }.
public static class Api
{
    private const int Puerto = 5080;

    // El emulador de Android llega a la PC por 10.0.2.2
    // IP de la PC donde corre el backend, en el Wi-Fi. Si cambia, hay que actualizarla acá (se ve con "ipconfig").
    private const string IpPc = "192.168.0.23";

    private static readonly string Servidor = $"http://{Host()}:{Puerto}/";

    // Windows: la misma PC. Celular: la PC por Wi-Fi. Emulador de Android: 10.0.2.2 es la PC.
    private static string Host()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
            return "localhost";
        return DeviceInfo.DeviceType == DeviceType.Virtual ? "10.0.2.2" : IpPc;
    }

    // 30 segundos para que alcance al subir una foto
    private static readonly HttpClient http = new() { BaseAddress = new Uri($"{Servidor}api/"), Timeout = TimeSpan.FromSeconds(30) };

    private static readonly JsonSerializerOptions opciones = new(JsonSerializerDefaults.Web) { Converters = { new ColorJson() } };

    // Usuario que inició sesión con correo y contraseña
    public static Usuario? UsuarioActual { get; set; }

    // Solo las asociaciones ven los casos que necesitan ayuda
    public static bool EsAsociacion => UsuarioActual?.Tipo == nameof(TipoCuenta.Asociacion);

    // Sube la foto y devuelve su ruta en el servidor
    public static async Task<string> SubirFoto(FileResult archivo)
    {
        await using var foto = await archivo.OpenReadAsync();
        using var formulario = new MultipartFormDataContent { { new StreamContent(foto), "archivo", archivo.FileName } };
        return await Enviar<string>(() => http.PostAsync("Caso/SubirFoto", formulario));
    }

    // Las imágenes están en la base y se piden al backend por su ruta (ej. "fotos/abc.jpg")
    public static string UrlFoto(string foto) => foto.StartsWith("fotos/") ? Servidor + foto : foto;

    // Los favoritos y las mascotas son de cada cuenta: la API necesita saber cuál es
    private static string ParametroUsuario => UsuarioActual is { } u ? $"?usuarioId={u.Id}" : "";

    // La ruta es el nombre de la clase (ej. "Campana/Lista")
    public static Task<List<T>> Lista<T>()
        => Enviar<List<T>>(() => http.GetAsync($"{typeof(T).Name}/Lista{ParametroUsuario}"));

    public static Task Guardar(string recurso, object datos)
        => Enviar<JsonElement>(() => http.PostAsJsonAsync($"{recurso}/Guardar", datos, opciones));

    public static Task Editar(string recurso, int id, object datos)
        => Enviar<JsonElement>(() => http.PutAsJsonAsync($"{recurso}/Editar/{id}", datos, opciones));

    public static Task Eliminar(string recurso, int id)
        => Enviar<JsonElement>(() => http.DeleteAsync($"{recurso}/Eliminar/{id}"));

    public static Task MarcarFavorito(ItemFavoritable item, bool valor)
        => Enviar<JsonElement>(() => http.PutAsync($"{item.GetType().Name}/Favorito/{item.Id}/{valor}{ParametroUsuario}", null));

    public static Task<Caso> GuardarCaso(Caso caso)
        => Enviar<Caso>(() => http.PostAsJsonAsync("Caso/Guardar", caso, opciones));

    public static Task<Usuario> Login(string correo, string clave)
        => Enviar<Usuario>(() => http.PostAsJsonAsync("Usuario/Login", new { correo, clave }, opciones));

    public static Task<Usuario> Registrar(object datos)
        => Enviar<Usuario>(() => http.PostAsJsonAsync("Usuario/Registrar", datos, opciones));

    private static async Task<T> Enviar<T>(Func<Task<HttpResponseMessage>> pedido)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await pedido();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException("No se pudo conectar con el servidor. Revisá que el backend esté encendido.");
        }

        Respuesta<T>? cuerpo = null;
        try
        {
            cuerpo = await respuesta.Content.ReadFromJsonAsync<Respuesta<T>>(opciones);
        }
        catch (JsonException)
        {
            // Sin cuerpo válido: se informa el error general abajo
        }

        if (!respuesta.IsSuccessStatusCode || cuerpo is null || cuerpo.Response is null)
            throw new ApiException(cuerpo?.Mensaje ?? "El servidor no pudo procesar el pedido.");

        return cuerpo.Response;
    }

    private record Respuesta<T>(string? Mensaje, T? Response);

    // Los colores viajan como texto ("#744DEF")
    private class ColorJson : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Color.FromArgb(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToHex());
    }
}
