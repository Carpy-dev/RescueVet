// Casos de la app: cada caso es una solicitud, con su animal, solicitante, estado, historial y foto.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;
using RescueVet.Api.Models;

namespace RescueVet.Api.Controllers;

public class CasoController(AppDbContext db) : FavoritoController<Solicitud, CasoDto>(db)
{
    private const long TamanoMaximo = 10 * 1024 * 1024;
    private const string CorreoAnonimo = "anonimo@rescuevet.invalid";
    private static readonly string[] Prioridades = ["Urgente", "Alta", "Normal"];

    private static readonly Dictionary<string, string> TiposFoto = new()
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    };

    private Dictionary<int, string> estados = [];

    protected override IQueryable<Solicitud> Consulta(int? usuarioId) => Db.Solicitudes
        .Include(s => s.Animal).Include(s => s.Solicitante).Include(s => s.Categoria).Include(s => s.Necesidades)
        .OrderByDescending(s => s.FechaCreacion).AsSplitQuery();

    protected override async Task PrepararLecturaAsync(int? usuarioId)
    {
        await base.PrepararLecturaAsync(usuarioId);
        estados = await Db.EstadosSolicitud.AsNoTracking().ToDictionaryAsync(e => e.IdEstado, e => e.Nombre);
    }

    protected override CasoDto AlDto(Solicitud s) => new()
    {
        Id = s.IdSolicitud,
        Nombre = s.Titulo,
        Foto = Ruta(s.IdFoto),
        EsFavorito = Favoritos.Contains(s.IdSolicitud),
        Ubicacion = s.Ubicacion ?? "",
        Descripcion = s.Descripcion ?? "",
        Categoria = s.Categoria?.Nombre ?? "",
        Especie = s.Animal?.Especie ?? "",
        Prioridad = s.Prioridad ?? "",
        Estado = estados.GetValueOrDefault(s.IdEstado, ""),
        Historia = s.Historia ?? "",
        PublicadoPor = s.Solicitante?.Nombre ?? "",
        Telefono = s.TelefonoContacto ?? "",
        Fecha = DateTime.SpecifyKind(s.FechaCreacion, DateTimeKind.Utc),
        Necesita = s.Necesidades.OrderBy(n => n.IdNecesidad).Select(n => n.Texto).ToList(),
    };

    protected override Task<string?> ValidarAsync(CasoDto d, bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(d.Nombre))
            return Task.FromResult<string?>("El nombre es obligatorio.");
        if (!string.IsNullOrEmpty(d.Prioridad) && !Prioridades.Contains(d.Prioridad))
            return Task.FromResult<string?>("La prioridad debe ser Urgente, Alta o Normal.");
        return Task.FromResult<string?>(null);
    }

    protected override async Task AplicarAsync(Solicitud s, CasoDto d, bool nuevo)
    {
        s.Titulo = d.Nombre;
        s.IdFoto = await IdFotoAsync(d.Foto);
        s.Ubicacion = d.Ubicacion;
        s.Descripcion = d.Descripcion;
        s.Historia = d.Historia;
        s.Prioridad = string.IsNullOrEmpty(d.Prioridad) ? null : d.Prioridad;
        s.TelefonoContacto = d.Telefono;
        s.IdCategoria = (await CategoriaAsync(d.Categoria)).IdCategoria;
        s.Necesidades = d.Necesita.Select(t => new SolicitudNecesidad { Texto = t }).ToList();

        if (!nuevo)
        {
            if (s.Animal is not null)
                s.Animal.Especie = string.IsNullOrEmpty(d.Especie) ? null : d.Especie;
            return;
        }

        var solicitante = await SolicitanteAsync(d.UsuarioId);
        var pendiente = await Db.EstadosSolicitud.FirstAsync(e => e.Nombre == "Pendiente");
        s.IdSolicitante = solicitante.IdSolicitante;
        s.IdEstado = pendiente.IdEstado;
        s.FechaCreacion = DateTime.UtcNow;
        s.Animal = new Animal { Especie = string.IsNullOrEmpty(d.Especie) ? null : d.Especie };

        // El historial se guarda en la misma operación que la solicitud
        s.Historial.Add(new HistorialEstado
        {
            IdEstado = pendiente.IdEstado,
            IdUsuario = solicitante.IdUsuario,
            FechaHora = s.FechaCreacion,
            Observacion = "Caso publicado",
        });
    }

    // El animal se elimina junto con su última solicitud, si no es la mascota de alguien
    protected override async Task DespuesDeEliminarAsync(Solicitud s)
    {
        var animal = s.Animal;
        if (animal is null
            || await Db.Solicitudes.AnyAsync(x => x.IdAnimal == animal.IdAnimal)
            || await Db.Mascotas.AnyAsync(m => m.IdAnimal == animal.IdAnimal))
            return;

        Db.Animales.Remove(animal);
        await Db.SaveChangesAsync();
    }

    private async Task<CategoriaSolicitud> CategoriaAsync(string? nombre)
    {
        string texto = string.IsNullOrWhiteSpace(nombre) ? "Rescate" : nombre.Trim();
        var categoria = await Db.CategoriasSolicitud.FirstOrDefaultAsync(c => c.Nombre == texto);
        if (categoria is null)
        {
            categoria = new CategoriaSolicitud { Nombre = texto };
            Db.CategoriasSolicitud.Add(categoria);
            await Db.SaveChangesAsync();
        }
        return categoria;
    }

    // Quien publica sin cuenta queda bajo un solicitante común "Anónimo"
    private async Task<Solicitante> SolicitanteAsync(int? usuarioId)
    {
        var propio = usuarioId is { } u ? await Db.Solicitantes.FirstOrDefaultAsync(s => s.IdUsuario == u) : null;
        return propio ?? await Db.Solicitantes.FirstAsync(s => s.Usuario!.Email == CorreoAnonimo);
    }

    protected override Task<List<int>> IdsFavoritosAsync(int u)
        => Db.FavoritosSolicitud.Where(f => f.IdUsuario == u).Select(f => f.IdSolicitud).ToListAsync();

    protected override Task<bool> ExisteAsync(int id) => Db.Solicitudes.AnyAsync(s => s.IdSolicitud == id);

    protected override async Task MarcarAsync(int u, int id, bool valor)
    {
        var fila = await Db.FavoritosSolicitud.FindAsync(u, id);
        if (valor && fila is null) Db.FavoritosSolicitud.Add(new FavoritoSolicitud { IdUsuario = u, IdSolicitud = id });
        if (!valor && fila is not null) Db.FavoritosSolicitud.Remove(fila);
    }

    // Guarda la foto en la base con un nombre nuevo y devuelve su ruta (ej. "fotos/abc.jpg")
    [HttpPost("SubirFoto")]
    [RequestSizeLimit(TamanoMaximo + 1024 * 1024)]
    public async Task<IActionResult> SubirFoto(IFormFile archivo)
    {
        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!TiposFoto.TryGetValue(extension, out var tipo))
            return BadRequest(new { mensaje = "La foto debe ser JPG, PNG o WEBP." });
        if (archivo.Length == 0 || archivo.Length > TamanoMaximo)
            return BadRequest(new { mensaje = "La foto debe pesar menos de 10 MB." });

        using var memoria = new MemoryStream();
        await archivo.CopyToAsync(memoria);

        var foto = new Foto { Nombre = $"{Guid.NewGuid():N}{extension}", ContentType = tipo, Datos = memoria.ToArray() };
        Db.Fotos.Add(foto);
        await Db.SaveChangesAsync();
        return Ok(new { mensaje = "ok", response = $"fotos/{foto.Nombre}" });
    }

    // Devuelve la imagen guardada en la base
    [HttpGet("/fotos/{nombre}")]
    public async Task<IActionResult> VerFoto(string nombre)
    {
        var foto = await Db.Fotos.AsNoTracking().FirstOrDefaultAsync(f => f.Nombre == nombre);
        return foto is null ? NoEncontrado() : File(foto.Datos, foto.ContentType);
    }
}
