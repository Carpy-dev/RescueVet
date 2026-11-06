// Datos que entran y salen de la API. La app trabaja con estas formas, sin conocer las tablas normalizadas.
namespace RescueVet.Api.Models;

public record EtiquetaDto(string Texto, string ColorTexto, string ColorFondo);

public record ServicioDto(string Nombre, string Precio);

public class AsociacionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Foto { get; set; } = "";
    public bool EsFavorito { get; set; }
    public string Distancia { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Icono { get; set; } = "";
    public string SobreNosotros { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public int Rescatados { get; set; }
    public int Adoptados { get; set; }
    public List<EtiquetaDto> Etiquetas { get; set; } = [];
    public List<string> Necesidades { get; set; } = [];
}

public class VeterinariaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Foto { get; set; } = "";
    public bool EsFavorito { get; set; }
    public string Distancia { get; set; } = "";
    public double Puntaje { get; set; }
    public int Resenas { get; set; }
    public string Descripcion { get; set; } = "";
    public string Icono { get; set; } = "";
    public bool Abierta24h { get; set; }
    public string SobreNosotros { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Telefono { get; set; } = "";
    public List<EtiquetaDto> Etiquetas { get; set; } = [];
    public List<ServicioDto> Servicios { get; set; } = [];
}

public class CasoDto
{
    public int Id { get; set; }

    // Cuenta que publica. Sin cuenta, el caso queda como "Anónimo".
    public int? UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Foto { get; set; } = "";
    public bool EsFavorito { get; set; }
    public string Ubicacion { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Especie { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Historia { get; set; } = "";
    public string PublicadoPor { get; set; } = "";
    public string Telefono { get; set; } = "";
    public DateTime Fecha { get; set; }
    public List<string> Necesita { get; set; } = [];

    public string Hace
    {
        get
        {
            var t = DateTime.UtcNow - Fecha;
            if (t.TotalMinutes < 1) return "Hace un momento";
            if (t.TotalHours < 1) return $"Hace {(int)t.TotalMinutes} min";
            if (t.TotalDays < 1) return (int)t.TotalHours == 1 ? "Hace 1 hora" : $"Hace {(int)t.TotalHours} horas";
            return (int)t.TotalDays == 1 ? "Hace 1 día" : $"Hace {(int)t.TotalDays} días";
        }
    }
}

public class RecordatorioDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public string Titulo { get; set; } = "";
    public string Frecuencia { get; set; } = "";
    public DateOnly Fecha { get; set; }
    public string Imagen { get; set; } = "";
    public string ColorTexto { get; set; } = "";
    public string ColorFondo { get; set; } = "";
}

public class RegistroMedicoDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Titulo { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string Lugar { get; set; } = "";
    public string Glifo { get; set; } = "";
    public string Color { get; set; } = "";
}

public class PesoDto
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public DateOnly Fecha { get; set; }
    public double Kg { get; set; }
}

public class MascotaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public string Raza { get; set; } = "";
    public string Edad { get; set; } = "";
    public string Sexo { get; set; } = "";
    public string RangoPeso { get; set; } = "";
    public string EstadoPeso { get; set; } = "";
    public string EstadoVacunas { get; set; } = "";
    public string Alergias { get; set; } = "";
    public string Foto { get; set; } = "";
    public List<string> Consejos { get; set; } = [];
    public List<string> Galeria { get; set; } = [];
    public List<RecordatorioDto> Recordatorios { get; set; } = [];
    public List<RegistroMedicoDto> Historial { get; set; } = [];
    public List<PesoDto> Pesos { get; set; } = [];
}

public class CampanaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Detalle { get; set; } = "";

    // Nombre de la asociación: se usa para abrir su perfil
    public string Asociacion { get; set; } = "";
    public string Foto { get; set; } = "";
    public string Recaudado { get; set; } = "";
    public decimal MontoRecaudado { get; set; }
    public int Porcentaje { get; set; }
}

public class NovedadDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string Foto { get; set; } = "";
}

// Lo que recibe /api/Usuario
public record RegistroRequest(string? Tipo, string? Nombre, string? Apellido, string? Documento,
    string? Matricula, string? Telefono, string? Ciudad, string? Correo, string? Clave);

public record LoginRequest(string? Correo, string? Clave);

// Lo que devuelve /api/Usuario (sin el hash)
public record UsuarioResponse(int Id, string Tipo, string Nombre, string Correo, string Telefono);
