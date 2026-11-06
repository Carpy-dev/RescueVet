// Asociaciones y veterinarias: el directorio que ve la app, con etiquetas, necesidades, servicios y favoritos.
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;
using RescueVet.Api.Models;

namespace RescueVet.Api.Controllers;

public static class Formato
{
    private static readonly CultureInfo Argentina = new("es-AR");

    public static string Pesos(decimal monto) => "$" + monto.ToString("N0", Argentina);

    // Convierte "$8.000" en 8000
    public static decimal LeerPesos(string? texto)
    {
        string digitos = new((texto ?? "").Where(char.IsDigit).ToArray());
        return decimal.TryParse(digitos, out var monto) ? monto : 0;
    }

    public static async Task<List<Etiqueta>> EtiquetasAsync(AppDbContext db, IEnumerable<EtiquetaDto> dtos)
    {
        var lista = new List<Etiqueta>();
        foreach (var d in dtos)
        {
            var etiqueta = await db.Etiquetas.FirstOrDefaultAsync(e => e.Texto == d.Texto && e.ColorTexto == d.ColorTexto && e.ColorFondo == d.ColorFondo)
                ?? new Etiqueta { Texto = d.Texto, ColorTexto = d.ColorTexto, ColorFondo = d.ColorFondo };
            lista.Add(etiqueta);
        }
        return lista;
    }
}

public class AsociacionController(AppDbContext db) : FavoritoController<Asociacion, AsociacionDto>(db)
{
    protected override IQueryable<Asociacion> Consulta(int? usuarioId) => Db.Asociaciones
        .Include(a => a.Etiquetas).Include(a => a.Necesidades)
        .Where(a => a.Activa).OrderBy(a => a.IdAsociacion).AsSplitQuery();

    protected override AsociacionDto AlDto(Asociacion a) => new()
    {
        Id = a.IdAsociacion,
        Nombre = a.Nombre,
        Foto = Ruta(a.IdFoto),
        Icono = Ruta(a.IdIcono),
        EsFavorito = Favoritos.Contains(a.IdAsociacion),
        Distancia = a.Distancia ?? "",
        Descripcion = a.Descripcion ?? "",
        SobreNosotros = a.SobreNosotros ?? "",
        Direccion = a.Direccion ?? "",
        Horario = a.Horario ?? "",
        Telefono = a.Telefono ?? "",
        Email = a.EmailContacto ?? "",
        Rescatados = a.Rescatados,
        Adoptados = a.Adoptados,
        Etiquetas = a.Etiquetas.OrderBy(e => e.IdEtiqueta).Select(e => new EtiquetaDto(e.Texto, e.ColorTexto, e.ColorFondo)).ToList(),
        Necesidades = a.Necesidades.OrderBy(n => n.IdNecesidad).Select(n => n.Texto).ToList(),
    };

    protected override Task<string?> ValidarAsync(AsociacionDto dto, bool nuevo)
        => Task.FromResult(string.IsNullOrWhiteSpace(dto.Nombre) ? "El nombre es obligatorio." : null);

    protected override async Task AplicarAsync(Asociacion a, AsociacionDto d, bool nuevo)
    {
        a.Nombre = d.Nombre;
        a.IdFoto = await IdFotoAsync(d.Foto);
        a.IdIcono = await IdFotoAsync(d.Icono);
        a.Distancia = d.Distancia;
        a.Descripcion = d.Descripcion;
        a.SobreNosotros = d.SobreNosotros;
        a.Direccion = d.Direccion;
        a.Horario = d.Horario;
        a.Telefono = d.Telefono;
        a.EmailContacto = d.Email;
        a.Rescatados = d.Rescatados;
        a.Adoptados = d.Adoptados;
        a.Etiquetas = await Formato.EtiquetasAsync(Db, d.Etiquetas);
        a.Necesidades = d.Necesidades.Select(t => new AsociacionNecesidad { Texto = t }).ToList();
    }

    protected override Task<List<int>> IdsFavoritosAsync(int u)
        => Db.FavoritosAsociacion.Where(f => f.IdUsuario == u).Select(f => f.IdAsociacion).ToListAsync();

    protected override Task<bool> ExisteAsync(int id) => Db.Asociaciones.AnyAsync(a => a.IdAsociacion == id);

    protected override async Task MarcarAsync(int u, int id, bool valor)
    {
        var fila = await Db.FavoritosAsociacion.FindAsync(u, id);
        if (valor && fila is null) Db.FavoritosAsociacion.Add(new FavoritoAsociacion { IdUsuario = u, IdAsociacion = id });
        if (!valor && fila is not null) Db.FavoritosAsociacion.Remove(fila);
    }
}

public class VeterinariaController(AppDbContext db) : FavoritoController<Veterinaria, VeterinariaDto>(db)
{
    protected override IQueryable<Veterinaria> Consulta(int? usuarioId) => Db.Veterinarias
        .Include(v => v.Etiquetas).Include(v => v.Servicios)
        .Where(v => v.Disponible).OrderBy(v => v.IdVeterinaria).AsSplitQuery();

    protected override VeterinariaDto AlDto(Veterinaria v) => new()
    {
        Id = v.IdVeterinaria,
        Nombre = v.Nombre,
        Foto = Ruta(v.IdFoto),
        Icono = Ruta(v.IdIcono),
        EsFavorito = Favoritos.Contains(v.IdVeterinaria),
        Distancia = v.Distancia ?? "",
        Puntaje = (double)v.Puntaje,
        Resenas = v.Resenas,
        Descripcion = v.Descripcion ?? "",
        Abierta24h = v.Abierta24h,
        SobreNosotros = v.SobreNosotros ?? "",
        Direccion = v.Direccion ?? "",
        Horario = v.Horario ?? "",
        Telefono = v.Telefono ?? "",
        Etiquetas = v.Etiquetas.OrderBy(e => e.IdEtiqueta).Select(e => new EtiquetaDto(e.Texto, e.ColorTexto, e.ColorFondo)).ToList(),
        Servicios = v.Servicios.OrderBy(s => s.IdServicio).Select(s => new ServicioDto(s.Nombre, Formato.Pesos(s.Precio))).ToList(),
    };

    protected override Task<string?> ValidarAsync(VeterinariaDto dto, bool nuevo)
        => Task.FromResult(string.IsNullOrWhiteSpace(dto.Nombre) ? "El nombre es obligatorio." : null);

    protected override async Task AplicarAsync(Veterinaria v, VeterinariaDto d, bool nuevo)
    {
        v.Nombre = d.Nombre;
        v.IdFoto = await IdFotoAsync(d.Foto);
        v.IdIcono = await IdFotoAsync(d.Icono);
        v.Distancia = d.Distancia;
        v.Puntaje = (decimal)d.Puntaje;
        v.Resenas = d.Resenas;
        v.Descripcion = d.Descripcion;
        v.Abierta24h = d.Abierta24h;
        v.SobreNosotros = d.SobreNosotros;
        v.Direccion = d.Direccion;
        v.Horario = d.Horario;
        v.Telefono = d.Telefono;
        v.Etiquetas = await Formato.EtiquetasAsync(Db, d.Etiquetas);
        v.Servicios = d.Servicios.Select(s => new Servicio { Nombre = s.Nombre, Precio = Formato.LeerPesos(s.Precio) }).ToList();
    }

    protected override Task<List<int>> IdsFavoritosAsync(int u)
        => Db.FavoritosVeterinaria.Where(f => f.IdUsuario == u).Select(f => f.IdVeterinaria).ToListAsync();

    protected override Task<bool> ExisteAsync(int id) => Db.Veterinarias.AnyAsync(v => v.IdVeterinaria == id);

    protected override async Task MarcarAsync(int u, int id, bool valor)
    {
        var fila = await Db.FavoritosVeterinaria.FindAsync(u, id);
        if (valor && fila is null) Db.FavoritosVeterinaria.Add(new FavoritoVeterinaria { IdUsuario = u, IdVeterinaria = id });
        if (!valor && fila is not null) Db.FavoritosVeterinaria.Remove(fila);
    }
}
