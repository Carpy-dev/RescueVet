// Controladores de Mi Mascota, campañas y novedades.
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;
using RescueVet.Api.Models;

namespace RescueVet.Api.Controllers;

public class MascotaController(AppDbContext db) : CrudController<Mascota, MascotaDto>(db)
{
    // Con usuarioId trae solo las mascotas de esa cuenta
    protected override IQueryable<Mascota> Consulta(int? usuarioId)
    {
        var consulta = Db.Mascotas
            .Include(m => m.Animal).Include(m => m.Consejos).Include(m => m.Galeria)
            .Include(m => m.Recordatorios).Include(m => m.Historial).Include(m => m.Pesos)
            .AsSplitQuery();
        if (usuarioId is { } u)
            consulta = consulta.Where(m => m.IdUsuario == u);
        return consulta.OrderBy(m => m.IdMascota);
    }

    protected override MascotaDto AlDto(Mascota m) => new()
    {
        Id = m.IdMascota,
        UsuarioId = m.IdUsuario,
        Nombre = m.Animal?.Nombre ?? "",
        Raza = m.Animal?.Raza ?? "",
        Edad = m.Animal?.EdadAproximada ?? "",
        Sexo = m.Animal?.Sexo ?? "",
        RangoPeso = m.RangoPeso ?? "",
        EstadoPeso = m.EstadoPeso ?? "",
        EstadoVacunas = m.EstadoVacunas ?? "",
        Alergias = m.Alergias ?? "",
        Foto = Ruta(m.IdFoto),
        Consejos = m.Consejos.OrderBy(c => c.IdConsejo).Select(c => c.Texto).ToList(),
        Galeria = m.Galeria.OrderBy(g => g.Orden).Select(g => Ruta(g.IdFoto)).ToList(),
        Recordatorios = m.Recordatorios.OrderBy(r => r.Fecha).Select(r => new RecordatorioDto
        {
            Id = r.IdRecordatorio, MascotaId = r.IdMascota, Titulo = r.Titulo, Frecuencia = r.Frecuencia ?? "",
            Fecha = r.Fecha, Imagen = Ruta(r.IdFoto), ColorTexto = r.ColorTexto, ColorFondo = r.ColorFondo,
        }).ToList(),
        Historial = m.Historial.OrderByDescending(h => h.Fecha).Select(h => new RegistroMedicoDto
        {
            Id = h.IdRegistro, MascotaId = h.IdMascota, Fecha = h.Fecha, Titulo = h.Titulo, Detalle = h.Detalle ?? "",
            Lugar = h.Lugar ?? "", Glifo = h.Glifo, Color = h.Color,
        }).ToList(),
        Pesos = m.Pesos.OrderByDescending(p => p.Fecha).Select(p => new PesoDto
        {
            Id = p.IdPeso, MascotaId = p.IdMascota, Fecha = p.Fecha, Kg = (double)p.Kg,
        }).ToList(),
    };

    protected override async Task<string?> ValidarAsync(MascotaDto d, bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(d.Nombre))
            return "El nombre es obligatorio.";
        if (nuevo && !await Db.Usuarios.AnyAsync(u => u.IdUsuario == d.UsuarioId && u.Activo))
            return "La mascota debe pertenecer a una cuenta.";
        return null;
    }

    protected override async Task AplicarAsync(Mascota m, MascotaDto d, bool nuevo)
    {
        if (nuevo)
        {
            m.IdUsuario = d.UsuarioId;
            m.Animal = new Animal();
        }

        m.Animal!.Nombre = d.Nombre;
        m.Animal.Raza = d.Raza;
        m.Animal.EdadAproximada = d.Edad;
        m.Animal.Sexo = d.Sexo;
        m.RangoPeso = d.RangoPeso;
        m.EstadoPeso = d.EstadoPeso;
        m.EstadoVacunas = d.EstadoVacunas;
        m.Alergias = d.Alergias;
        m.IdFoto = await IdFotoAsync(string.IsNullOrEmpty(d.Foto) ? "rv_masc_max.png" : d.Foto);
        m.Consejos = d.Consejos.Select(t => new MascotaConsejo { Texto = t }).ToList();

        var galeria = new List<MascotaGaleria>();
        foreach (var ruta in d.Galeria)
            if (await IdFotoAsync(ruta) is { } id && galeria.All(g => g.IdFoto != id))
                galeria.Add(new MascotaGaleria { IdFoto = id, Orden = galeria.Count + 1 });
        m.Galeria = galeria;
    }

    // El animal de la mascota se elimina con ella, salvo que tenga solicitudes
    protected override async Task DespuesDeEliminarAsync(Mascota m)
    {
        var animal = m.Animal;
        if (animal is null || await Db.Solicitudes.AnyAsync(s => s.IdAnimal == animal.IdAnimal))
            return;

        Db.Animales.Remove(animal);
        await Db.SaveChangesAsync();
    }
}

public class RecordatorioController(AppDbContext db) : CrudController<Recordatorio, RecordatorioDto>(db)
{
    protected override IQueryable<Recordatorio> Consulta(int? usuarioId) => Db.Recordatorios.OrderBy(r => r.Fecha);

    protected override RecordatorioDto AlDto(Recordatorio r) => new()
    {
        Id = r.IdRecordatorio, MascotaId = r.IdMascota, Titulo = r.Titulo, Frecuencia = r.Frecuencia ?? "",
        Fecha = r.Fecha, Imagen = Ruta(r.IdFoto), ColorTexto = r.ColorTexto, ColorFondo = r.ColorFondo,
    };

    protected override async Task<string?> ValidarAsync(RecordatorioDto d, bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(d.Titulo)) return "El título es obligatorio.";
        return await Db.Mascotas.AnyAsync(m => m.IdMascota == d.MascotaId) ? null : "La mascota no existe.";
    }

    protected override async Task AplicarAsync(Recordatorio r, RecordatorioDto d, bool nuevo)
    {
        r.IdMascota = d.MascotaId;
        r.Titulo = d.Titulo;
        r.Frecuencia = d.Frecuencia;
        r.Fecha = d.Fecha;
        r.IdFoto = await IdFotoAsync(string.IsNullOrEmpty(d.Imagen) ? "rv_masc_ic_record.png" : d.Imagen);
        r.ColorTexto = string.IsNullOrEmpty(d.ColorTexto) ? "#744DEF" : d.ColorTexto;
        r.ColorFondo = string.IsNullOrEmpty(d.ColorFondo) ? "#F0EAFD" : d.ColorFondo;
    }
}

public class RegistroMedicoController(AppDbContext db) : CrudController<RegistroMedico, RegistroMedicoDto>(db)
{
    protected override IQueryable<RegistroMedico> Consulta(int? usuarioId) => Db.RegistrosMedicos.OrderByDescending(h => h.Fecha);

    protected override RegistroMedicoDto AlDto(RegistroMedico h) => new()
    {
        Id = h.IdRegistro, MascotaId = h.IdMascota, Fecha = h.Fecha, Titulo = h.Titulo, Detalle = h.Detalle ?? "",
        Lugar = h.Lugar ?? "", Glifo = h.Glifo, Color = h.Color,
    };

    protected override async Task<string?> ValidarAsync(RegistroMedicoDto d, bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(d.Titulo)) return "El título es obligatorio.";
        return await Db.Mascotas.AnyAsync(m => m.IdMascota == d.MascotaId) ? null : "La mascota no existe.";
    }

    protected override Task AplicarAsync(RegistroMedico h, RegistroMedicoDto d, bool nuevo)
    {
        h.IdMascota = d.MascotaId;
        h.Fecha = d.Fecha;
        h.Titulo = d.Titulo;
        h.Detalle = d.Detalle;
        h.Lugar = d.Lugar;
        h.Glifo = string.IsNullOrEmpty(d.Glifo) ? "" : d.Glifo;
        h.Color = string.IsNullOrEmpty(d.Color) ? "#1A7CF0" : d.Color;
        return Task.CompletedTask;
    }
}

public class PesoController(AppDbContext db) : CrudController<Peso, PesoDto>(db)
{
    protected override IQueryable<Peso> Consulta(int? usuarioId) => Db.Pesos.OrderByDescending(p => p.Fecha);

    protected override PesoDto AlDto(Peso p) => new() { Id = p.IdPeso, MascotaId = p.IdMascota, Fecha = p.Fecha, Kg = (double)p.Kg };

    protected override async Task<string?> ValidarAsync(PesoDto d, bool nuevo)
    {
        if (d.Kg <= 0) return "Ingresá un peso mayor a 0.";
        return await Db.Mascotas.AnyAsync(m => m.IdMascota == d.MascotaId) ? null : "La mascota no existe.";
    }

    protected override Task AplicarAsync(Peso p, PesoDto d, bool nuevo)
    {
        p.IdMascota = d.MascotaId;
        p.Fecha = d.Fecha;
        p.Kg = (decimal)d.Kg;
        return Task.CompletedTask;
    }
}

public class CampanaController(AppDbContext db) : CrudController<Campana, CampanaDto>(db)
{
    protected override IQueryable<Campana> Consulta(int? usuarioId) => Db.Campanas.Include(c => c.Asociacion).OrderBy(c => c.IdCampana);

    protected override CampanaDto AlDto(Campana c) => new()
    {
        Id = c.IdCampana,
        Titulo = c.Titulo,
        Detalle = c.Detalle ?? "",
        Asociacion = c.Asociacion?.Nombre ?? "",
        Foto = Ruta(c.IdFoto),
        Recaudado = $"{Formato.Pesos(c.MontoRecaudado)} recaudados",
        MontoRecaudado = c.MontoRecaudado,
        Porcentaje = c.Porcentaje,
    };

    protected override async Task<string?> ValidarAsync(CampanaDto c, bool nuevo)
    {
        if (string.IsNullOrWhiteSpace(c.Titulo)) return "El título es obligatorio.";
        if (c.Porcentaje is < 0 or > 100) return "El porcentaje debe estar entre 0 y 100.";
        return await Db.Asociaciones.AnyAsync(a => a.Nombre == c.Asociacion) ? null : "La asociación no existe.";
    }

    protected override async Task AplicarAsync(Campana c, CampanaDto d, bool nuevo)
    {
        c.Titulo = d.Titulo;
        c.Detalle = d.Detalle;
        c.IdAsociacion = await Db.Asociaciones.Where(a => a.Nombre == d.Asociacion).Select(a => a.IdAsociacion).FirstAsync();
        c.IdFoto = await IdFotoAsync(d.Foto);
        c.MontoRecaudado = d.MontoRecaudado;
        c.Porcentaje = d.Porcentaje;
    }
}

public class NovedadController(AppDbContext db) : CrudController<Novedad, NovedadDto>(db)
{
    protected override IQueryable<Novedad> Consulta(int? usuarioId) => Db.Novedades.OrderByDescending(n => n.IdNovedad);

    protected override NovedadDto AlDto(Novedad n) => new()
    {
        Id = n.IdNovedad, Titulo = n.Titulo, Descripcion = n.Descripcion ?? "", Detalle = n.Detalle ?? "", Foto = Ruta(n.IdFoto),
    };

    protected override Task<string?> ValidarAsync(NovedadDto n, bool nuevo)
        => Task.FromResult(string.IsNullOrWhiteSpace(n.Titulo) ? "El título es obligatorio." : null);

    protected override async Task AplicarAsync(Novedad n, NovedadDto d, bool nuevo)
    {
        n.Titulo = d.Titulo;
        n.Descripcion = d.Descripcion;
        n.Detalle = d.Detalle;
        n.IdFoto = await IdFotoAsync(d.Foto);
    }
}
