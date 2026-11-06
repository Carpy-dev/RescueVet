// Conexión con la base mediante Entity Framework: define las tablas, sus relaciones y sus reglas.
// Las clases se llaman en PascalCase y en la base quedan como tabla_columna (ej. IdUsuario es id_usuario).
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Models;

namespace RescueVet.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Solicitante> Solicitantes => Set<Solicitante>();
    public DbSet<Asociacion> Asociaciones => Set<Asociacion>();
    public DbSet<AsociacionNecesidad> AsociacionNecesidades => Set<AsociacionNecesidad>();
    public DbSet<Veterinaria> Veterinarias => Set<Veterinaria>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Etiqueta> Etiquetas => Set<Etiqueta>();
    public DbSet<Animal> Animales => Set<Animal>();
    public DbSet<EstadoSolicitud> EstadosSolicitud => Set<EstadoSolicitud>();
    public DbSet<CategoriaSolicitud> CategoriasSolicitud => Set<CategoriaSolicitud>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<SolicitudNecesidad> SolicitudNecesidades => Set<SolicitudNecesidad>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<Atencion> Atenciones => Set<Atencion>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<MascotaConsejo> MascotaConsejos => Set<MascotaConsejo>();
    public DbSet<MascotaGaleria> MascotaGalerias => Set<MascotaGaleria>();
    public DbSet<Recordatorio> Recordatorios => Set<Recordatorio>();
    public DbSet<RegistroMedico> RegistrosMedicos => Set<RegistroMedico>();
    public DbSet<Peso> Pesos => Set<Peso>();
    public DbSet<Campana> Campanas => Set<Campana>();
    public DbSet<Novedad> Novedades => Set<Novedad>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<FavoritoAsociacion> FavoritosAsociacion => Set<FavoritoAsociacion>();
    public DbSet<FavoritoVeterinaria> FavoritosVeterinaria => Set<FavoritoVeterinaria>();
    public DbSet<FavoritoSolicitud> FavoritosSolicitud => Set<FavoritoSolicitud>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        // ----- Cuentas -----
        m.Entity<Rol>().HasIndex(r => r.Nombre).IsUnique();
        m.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        m.Entity<Usuario>().Property(u => u.Activo).HasDefaultValue(true);
        m.Entity<Usuario>().Property(u => u.FechaRegistro).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        m.Entity<Solicitante>().HasIndex(s => s.IdUsuario).IsUnique();
        m.Entity<Solicitante>().HasIndex(s => s.Documento).IsUnique().HasFilter("[documento] IS NOT NULL");

        // ----- Directorio: un usuario tiene 0 o 1 perfil, y el perfil puede no tener cuenta -----
        m.Entity<Asociacion>().HasOne<Usuario>().WithOne().HasForeignKey<Asociacion>(a => a.IdUsuario);
        m.Entity<Asociacion>().HasOne<Foto>().WithMany().HasForeignKey(a => a.IdFoto);
        m.Entity<Asociacion>().HasOne<Foto>().WithMany().HasForeignKey(a => a.IdIcono);
        m.Entity<Veterinaria>().HasOne<Usuario>().WithOne().HasForeignKey<Veterinaria>(v => v.IdUsuario);
        m.Entity<Veterinaria>().HasOne<Foto>().WithMany().HasForeignKey(v => v.IdFoto);
        m.Entity<Veterinaria>().HasOne<Foto>().WithMany().HasForeignKey(v => v.IdIcono);
        m.Entity<Veterinaria>().ToTable(t => t.HasCheckConstraint("CK_veterinaria_puntaje", "[puntaje] BETWEEN 0 AND 5"));
        m.Entity<Etiqueta>().HasIndex(e => new { e.Texto, e.ColorTexto, e.ColorFondo }).IsUnique();
        m.Entity<Asociacion>().HasMany(a => a.Etiquetas).WithMany().UsingEntity("AsociacionEtiqueta");
        m.Entity<Veterinaria>().HasMany(v => v.Etiquetas).WithMany().UsingEntity("VeterinariaEtiqueta");
        m.Entity<Servicio>().ToTable(t => t.HasCheckConstraint("CK_servicio_precio", "[precio] >= 0"));

        // ----- Solicitudes -----
        m.Entity<EstadoSolicitud>().HasIndex(e => e.Nombre).IsUnique();
        m.Entity<CategoriaSolicitud>().HasIndex(c => c.Nombre).IsUnique();
        m.Entity<Solicitud>().HasOne<Asociacion>().WithMany().HasForeignKey(s => s.IdAsociacion);
        m.Entity<Solicitud>().HasOne<EstadoSolicitud>().WithMany().HasForeignKey(s => s.IdEstado);
        m.Entity<Solicitud>().HasOne<Foto>().WithMany().HasForeignKey(s => s.IdFoto);
        m.Entity<Solicitud>().Property(s => s.FechaCreacion).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        m.Entity<Solicitud>().HasIndex(s => new { s.IdEstado, s.FechaCreacion });
        m.Entity<Solicitud>().ToTable(t => t.HasCheckConstraint("CK_solicitud_prioridad", "[prioridad] IS NULL OR [prioridad] IN ('Urgente','Alta','Normal')"));
        m.Entity<Asignacion>().HasOne<Solicitud>().WithMany().HasForeignKey(a => a.IdSolicitud);
        m.Entity<Asignacion>().HasOne<Veterinaria>().WithMany().HasForeignKey(a => a.IdVeterinaria);
        m.Entity<Asignacion>().Property(a => a.FechaAsignacion).HasColumnType("datetime2(0)");
        m.Entity<Atencion>().HasOne<Asignacion>().WithOne().HasForeignKey<Atencion>(a => a.IdAsignacion);
        m.Entity<Atencion>().Property(a => a.FechaAtencion).HasColumnType("datetime2(0)");
        m.Entity<Atencion>().ToTable(t => t.HasCheckConstraint("CK_atencion_costo", "[costo] >= 0"));
        m.Entity<HistorialEstado>().HasOne<EstadoSolicitud>().WithMany().HasForeignKey(h => h.IdEstado);
        m.Entity<HistorialEstado>().HasOne<Usuario>().WithMany().HasForeignKey(h => h.IdUsuario);
        m.Entity<HistorialEstado>().Property(h => h.FechaHora).HasColumnType("datetime2(0)");

        // ----- Mi Mascota -----
        m.Entity<Mascota>().HasIndex(x => x.IdAnimal).IsUnique();
        m.Entity<Mascota>().HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario);
        m.Entity<Mascota>().HasOne<Foto>().WithMany().HasForeignKey(x => x.IdFoto);
        m.Entity<MascotaGaleria>().HasKey(g => new { g.IdMascota, g.IdFoto });
        m.Entity<MascotaGaleria>().HasOne<Foto>().WithMany().HasForeignKey(g => g.IdFoto);
        m.Entity<Recordatorio>().HasOne<Foto>().WithMany().HasForeignKey(r => r.IdFoto);
        m.Entity<Peso>().ToTable(t => t.HasCheckConstraint("CK_peso_kg", "[kg] > 0"));

        // ----- Contenido -----
        m.Entity<Campana>().HasOne<Foto>().WithMany().HasForeignKey(c => c.IdFoto);
        m.Entity<Campana>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_campana_porcentaje", "[porcentaje] BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_campana_monto", "[monto_recaudado] >= 0");
        });
        m.Entity<Novedad>().HasOne<Foto>().WithMany().HasForeignKey(n => n.IdFoto);
        m.Entity<Foto>().HasIndex(f => f.Nombre).IsUnique();

        // ----- Favoritos de cada usuario -----
        m.Entity<FavoritoAsociacion>().HasKey(f => new { f.IdUsuario, f.IdAsociacion });
        m.Entity<FavoritoAsociacion>().HasOne<Usuario>().WithMany().HasForeignKey(f => f.IdUsuario);
        m.Entity<FavoritoAsociacion>().HasOne<Asociacion>().WithMany().HasForeignKey(f => f.IdAsociacion);
        m.Entity<FavoritoVeterinaria>().HasKey(f => new { f.IdUsuario, f.IdVeterinaria });
        m.Entity<FavoritoVeterinaria>().HasOne<Usuario>().WithMany().HasForeignKey(f => f.IdUsuario);
        m.Entity<FavoritoVeterinaria>().HasOne<Veterinaria>().WithMany().HasForeignKey(f => f.IdVeterinaria);
        m.Entity<FavoritoSolicitud>().HasKey(f => new { f.IdUsuario, f.IdSolicitud });
        m.Entity<FavoritoSolicitud>().HasOne<Usuario>().WithMany().HasForeignKey(f => f.IdUsuario);
        m.Entity<FavoritoSolicitud>().HasOne<Solicitud>().WithMany().HasForeignKey(f => f.IdSolicitud);

        AplicarBorrados(m);
        AplicarNombres(m);
    }

    // Borrar un dato principal se rechaza si otro depende de él, salvo en sus detalles propios
    private static void AplicarBorrados(ModelBuilder m)
    {
        Type[] detalles =
        [
            typeof(AsociacionNecesidad), typeof(Servicio), typeof(SolicitudNecesidad), typeof(HistorialEstado),
            typeof(MascotaConsejo), typeof(MascotaGaleria), typeof(Recordatorio), typeof(RegistroMedico), typeof(Peso),
            typeof(FavoritoAsociacion), typeof(FavoritoVeterinaria), typeof(FavoritoSolicitud),
        ];
        Type[] principales = [typeof(Asociacion), typeof(Veterinaria), typeof(Solicitud), typeof(Mascota)];

        foreach (var entidad in m.Model.GetEntityTypes())
            foreach (var fk in entidad.GetForeignKeys())
            {
                bool esDetalle = detalles.Contains(entidad.ClrType) && principales.Contains(fk.PrincipalEntityType.ClrType);
                bool esEtiqueta = entidad.ClrType == typeof(Dictionary<string, object>);
                fk.DeleteBehavior = esDetalle || esEtiqueta ? DeleteBehavior.Cascade : DeleteBehavior.Restrict;
            }
    }

    private static void AplicarNombres(ModelBuilder m)
    {
        foreach (var entidad in m.Model.GetEntityTypes())
        {
            entidad.SetTableName(Tabla(entidad.GetTableName() ?? entidad.Name));
            foreach (var p in entidad.GetProperties())
                p.SetColumnName(Tabla(p.Name));
        }
    }

    // "IdUsuario" -> "id_usuario", "Abierta24h" -> "abierta_24h"
    private static string Tabla(string nombre)
    {
        nombre = nombre switch
        {
            "Roles" => "Rol", "Usuarios" => "Usuario", "Solicitantes" => "Solicitante", "Asociaciones" => "Asociacion",
            "AsociacionNecesidades" => "AsociacionNecesidad", "Veterinarias" => "Veterinaria", "Servicios" => "Servicio",
            "Etiquetas" => "Etiqueta", "Animales" => "Animal", "EstadosSolicitud" => "EstadoSolicitud",
            "CategoriasSolicitud" => "CategoriaSolicitud", "Solicitudes" => "Solicitud", "SolicitudNecesidades" => "SolicitudNecesidad",
            "Asignaciones" => "Asignacion", "Atenciones" => "Atencion", "HistorialEstados" => "HistorialEstado",
            "Mascotas" => "Mascota", "MascotaConsejos" => "MascotaConsejo", "MascotaGalerias" => "MascotaGaleria",
            "Recordatorios" => "Recordatorio", "RegistrosMedicos" => "RegistroMedico", "Pesos" => "Peso",
            "Campanas" => "Campana", "Novedades" => "Novedad", "Fotos" => "Foto",
            "FavoritosAsociacion" => "FavoritoAsociacion", "FavoritosVeterinaria" => "FavoritoVeterinaria", "FavoritosSolicitud" => "FavoritoSolicitud",
            "EtiquetasIdEtiqueta" => "IdEtiqueta",
            _ => nombre,
        };
        if (nombre.EndsWith("IdAsociacion") && nombre.Length > "IdAsociacion".Length) nombre = "IdAsociacion";
        if (nombre.EndsWith("IdVeterinaria") && nombre.Length > "IdVeterinaria".Length) nombre = "IdVeterinaria";
        return Regex.Replace(nombre, "(?<=[a-z0-9])([A-Z])|(?<=[a-z])([0-9])", "_$1$2").ToLowerInvariant();
    }
}
