// Modelo de una asociación tal como la devuelve la API. Se muestra en Asociaciones y en su perfil.
namespace Demo.Models;

public class Asociacion : ItemFavoritable
{
    public string Distancia { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Icono { get; set; } = "";
    public string IconoUrl => Api.UrlFoto(Icono);
    public Etiqueta[] Etiquetas { get; set; } = [];

    // Datos del perfil (pantalla de detalle)
    public string SobreNosotros { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public int Rescatados { get; set; }
    public int Adoptados { get; set; }
    public string[] Necesidades { get; set; } = [];

    public override string Resumen => $"Asociación · {Distancia}";
}
