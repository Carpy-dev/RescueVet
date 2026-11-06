// Modelo de una veterinaria tal como la devuelve la API, con sus servicios y precios.
namespace Demo.Models;

public record Servicio(string Nombre, string Precio);

public class Veterinaria : ItemFavoritable
{
    public string Distancia { get; set; } = "";
    public double Puntaje { get; set; }
    public int Resenas { get; set; }
    public string Descripcion { get; set; } = "";
    public string Icono { get; set; } = "";
    public string IconoUrl => Api.UrlFoto(Icono);
    public Etiqueta[] Etiquetas { get; set; } = [];
    public bool Abierta24h { get; set; }

    // Datos del perfil (pantalla de detalle)
    public string SobreNosotros { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Telefono { get; set; } = "";
    public Servicio[] Servicios { get; set; } = [];

    public string PuntajeTexto => $"{Puntaje:0.0}";
    public string ResenasTexto => $"({Resenas})";
    public string ResenasLargo => $"{Resenas} reseñas";
    public override string Resumen => $"Veterinaria · {Distancia}";
}
