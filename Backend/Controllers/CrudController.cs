// Rutas comunes de cada recurso: Lista, Obtener, Guardar, Editar y Eliminar, en /api/{Recurso}/...
// Cada controlador convierte entre las tablas normalizadas y el DTO que usa la app.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;

namespace RescueVet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntidad, TDto>(AppDbContext db) : ControllerBase
    where TEntidad : class, new()
{
    private Dictionary<int, string> nombresFoto = [];

    protected AppDbContext Db => db;

    // Cada recurso define qué datos relacionados trae y en qué orden
    protected abstract IQueryable<TEntidad> Consulta(int? usuarioId);
    protected abstract TDto AlDto(TEntidad entidad);
    protected abstract Task<string?> ValidarAsync(TDto dto, bool nuevo);
    protected abstract Task AplicarAsync(TEntidad entidad, TDto dto, bool nuevo);

    // Se ejecuta antes de leer (por ejemplo, para saber los favoritos del usuario)
    protected virtual Task PrepararLecturaAsync(int? usuarioId) => Task.CompletedTask;
    protected virtual Task DespuesDeEliminarAsync(TEntidad entidad) => Task.CompletedTask;

    [HttpGet("Lista")]
    public async Task<IActionResult> Lista([FromQuery] int? usuarioId)
    {
        await CargarFotosAsync();
        await PrepararLecturaAsync(usuarioId);
        var filas = await Consulta(usuarioId).AsNoTracking().ToListAsync();
        return Ok(new { mensaje = "ok", response = filas.Select(AlDto).ToList() });
    }

    [HttpGet("Obtener/{id:int}")]
    public async Task<IActionResult> Obtener(int id, [FromQuery] int? usuarioId)
    {
        var fila = await PorId(id).AsNoTracking().FirstOrDefaultAsync();
        if (fila is null)
            return NoEncontrado();

        await CargarFotosAsync();
        await PrepararLecturaAsync(usuarioId);
        return Ok(new { mensaje = "ok", response = AlDto(fila) });
    }

    [HttpPost("Guardar")]
    public async Task<IActionResult> Guardar([FromBody] TDto dto)
    {
        if (await ValidarAsync(dto, true) is { } error)
            return BadRequest(new { mensaje = error });

        var fila = new TEntidad();
        await AplicarAsync(fila, dto, true);
        db.Set<TEntidad>().Add(fila);
        await db.SaveChangesAsync();
        return await Obtener(Clave(fila), null);
    }

    [HttpPut("Editar/{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] TDto dto)
    {
        if (await ValidarAsync(dto, false) is { } error)
            return BadRequest(new { mensaje = error });

        var fila = await PorId(id).FirstOrDefaultAsync();
        if (fila is null)
            return NoEncontrado();

        await AplicarAsync(fila, dto, false);
        await db.SaveChangesAsync();
        return await Obtener(id, null);
    }

    [HttpDelete("Eliminar/{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var fila = await PorId(id).FirstOrDefaultAsync();
        if (fila is null)
            return NoEncontrado();

        try
        {
            db.Set<TEntidad>().Remove(fila);
            await db.SaveChangesAsync();
            await DespuesDeEliminarAsync(fila);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensaje = "No se puede eliminar porque tiene datos relacionados." });
        }
        return Ok(new { mensaje = "ok", response = id });
    }

    protected NotFoundObjectResult NoEncontrado() => NotFound(new { mensaje = "No encontrado." });

    // Las tablas guardan el id de la foto; la app recibe su ruta (ej. "fotos/abc.jpg")
    protected string Ruta(int? idFoto)
        => idFoto is { } id && nombresFoto.TryGetValue(id, out var nombre) ? $"fotos/{nombre}" : "";

    protected async Task<int?> IdFotoAsync(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return null;

        string nombre = ruta.Replace("fotos/", "");
        var id = await db.Fotos.Where(f => f.Nombre == nombre).Select(f => (int?)f.IdFoto).FirstOrDefaultAsync();
        return id;
    }

    // Solo se traen los nombres: los bytes de las imágenes se piden aparte
    private async Task CargarFotosAsync()
        => nombresFoto = await db.Fotos.AsNoTracking().ToDictionaryAsync(f => f.IdFoto, f => f.Nombre);

    private string NombreClave => db.Model.FindEntityType(typeof(TEntidad))!.FindPrimaryKey()!.Properties[0].Name;

    private IQueryable<TEntidad> PorId(int id)
    {
        string clave = NombreClave;
        return Consulta(null).Where(e => EF.Property<int>(e, clave) == id);
    }

    private int Clave(TEntidad fila) => (int)db.Entry(fila).Property(NombreClave).CurrentValue!;
}

// Asociaciones, veterinarias y casos: cada usuario puede guardar los suyos como favoritos
public abstract class FavoritoController<TEntidad, TDto>(AppDbContext db) : CrudController<TEntidad, TDto>(db)
    where TEntidad : class, new()
{
    protected HashSet<int> Favoritos { get; private set; } = [];

    protected abstract Task<List<int>> IdsFavoritosAsync(int usuarioId);
    protected abstract Task<bool> ExisteAsync(int id);
    protected abstract Task MarcarAsync(int usuarioId, int id, bool valor);

    protected override async Task PrepararLecturaAsync(int? usuarioId)
        => Favoritos = usuarioId is { } u ? [.. await IdsFavoritosAsync(u)] : [];

    [HttpPut("Favorito/{id:int}/{valor:bool}")]
    public async Task<IActionResult> Favorito(int id, bool valor, [FromQuery] int? usuarioId)
    {
        if (usuarioId is not { } u || !await Db.Usuarios.AnyAsync(x => x.IdUsuario == u && x.Activo))
            return BadRequest(new { mensaje = "Iniciá sesión con tu cuenta para guardar favoritos." });
        if (!await ExisteAsync(id))
            return NoEncontrado();

        await MarcarAsync(u, id, valor);
        await Db.SaveChangesAsync();
        return Ok(new { mensaje = "ok", response = valor });
    }
}
