// Modelo de un caso de ayuda: lo crea Reportar caso y lo ven las asociaciones en su panel.
namespace Demo.Models;

public class Caso : ItemFavoritable
{
    // Cuenta que publica el caso
    public int? UsuarioId { get; set; }
    public string Ubicacion { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Hace { get; set; } = "";
    public string Especie { get; set; } = "";
    public string Prioridad { get; set; } = "";

    // Datos de la pantalla de detalle
    public string Historia { get; set; } = "";
    public string PublicadoPor { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string[] Necesita { get; set; } = [];

    public override string Resumen => $"Caso · {Ubicacion}";
}
