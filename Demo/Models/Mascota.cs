// Modelos de Mi Mascota (perfil, recordatorios, historial y pesos), de las campañas y de las novedades.
// Las propiedades que terminan en Url o Texto se calculan para mostrar, no se guardan en la base.
using System.Globalization;

namespace Demo.Models;

// Datos de "Mi Mascota" y sus sub pantallas (vienen de api/Mascota/Lista)
public class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Raza { get; set; } = "";
    public string Edad { get; set; } = "";
    public string Sexo { get; set; } = "";
    public string RangoPeso { get; set; } = "";
    public string EstadoPeso { get; set; } = "";
    public string EstadoVacunas { get; set; } = "";
    public string Alergias { get; set; } = "";
    public string Foto { get; set; } = "";
    public string FotoUrl => Api.UrlFoto(Foto);
    public string[] Consejos { get; set; } = [];
    public string[] Galeria { get; set; } = [];
    public string[] GaleriaUrls => Galeria.Select(Api.UrlFoto).ToArray();
    public List<Recordatorio> Recordatorios { get; set; } = [];
    public List<RegistroMedico> Historial { get; set; } = [];
    public List<Peso> Pesos { get; set; } = [];

    // La API devuelve primero lo más reciente
    public string PesoActual => Pesos.FirstOrDefault()?.KgTexto ?? "-";
    public string UltimaVisita => Historial.FirstOrDefault()?.FechaTexto ?? "-";
    public string RecordatoriosTexto => $"{Recordatorios.Count} pendientes";
    public string ConsejosTexto => $"Cuidados y recomendaciones según la edad, raza y necesidades de {Nombre}.";
}

public record Recordatorio(string Titulo, string Frecuencia, DateOnly Fecha, string Imagen, Color ColorTexto, Color ColorFondo)
{
    public int Id { get; init; }
    public int MascotaId { get; init; }
    public string FechaTexto => TextoFecha.Dia(Fecha);
    public string ImagenUrl => Api.UrlFoto(Imagen);

    // Se calcula con la fecha de hoy
    public string Falta => (Fecha.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber) switch
    {
        < 0 => "Vencido",
        0 => "Hoy",
        1 => "Mañana",
        var dias => $"En {dias} días",
    };
}

public record RegistroMedico(DateOnly Fecha, string Titulo, string Detalle, string Lugar, string Glifo, Color Color)
{
    public int Id { get; init; }
    public int MascotaId { get; init; }
    public string FechaTexto => TextoFecha.Dia(Fecha);
}

public record Peso(DateOnly Fecha, double Kg)
{
    public int Id { get; init; }
    public int MascotaId { get; init; }
    public string FechaTexto => TextoFecha.Mes(Fecha);

    // Ancho de la barra en el gráfico de crecimiento (35 kg = barra completa)
    public double AnchoBarra => Kg / 35.0 * 220;
    public string KgTexto => $"{Kg:0.#} kg";
}

public static class TextoFecha
{
    private static readonly CultureInfo Español = new("es-ES");

    public static string Dia(DateOnly fecha) => fecha.ToString("d MMM yyyy", Español);
    public static string Mes(DateOnly fecha) => fecha.ToString("MMM yyyy", Español);
}

// Campañas de la pantalla de Donaciones
public record Campana(string Titulo, string Detalle, string Asociacion, string Foto, string Recaudado, int Porcentaje)
{
    public string FotoUrl => Api.UrlFoto(Foto);
    public double Progreso => Porcentaje / 100.0;
    public string PorcentajeTexto => $"{Porcentaje}%";
}

// Novedades de la pantalla de Inicio
public record Novedad(string Titulo, string Descripcion, string Detalle, string Foto)
{
    public string FotoUrl => Api.UrlFoto(Foto);
}
