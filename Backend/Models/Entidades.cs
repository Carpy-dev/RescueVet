// Modelos del backend: cada clase representa una tabla normalizada de la base.
// Los nombres de tablas y columnas se convierten a formato tabla_columna en AppDbContext.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RescueVet.Api.Models;

// ===================== Cuentas =====================

public class Rol
{
    [Key] public int IdRol { get; set; }
    [MaxLength(30)] public string Nombre { get; set; } = "";
}

public class Usuario
{
    [Key] public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    [ForeignKey(nameof(IdRol))] public Rol? Rol { get; set; }
    [MaxLength(254)] public string Email { get; set; } = "";

    // Sal y hash de la contraseña. Vacío en cuentas sin acceso (activo = 0).
    [MaxLength(255)] public string PasswordHash { get; set; } = "";
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
}

public class Solicitante
{
    [Key] public int IdSolicitante { get; set; }
    public int IdUsuario { get; set; }
    [ForeignKey(nameof(IdUsuario))] public Usuario? Usuario { get; set; }
    [MaxLength(80)] public string Nombre { get; set; } = "";
    [MaxLength(80)] public string? Apellido { get; set; }
    [MaxLength(20)] public string? Documento { get; set; }
    [MaxLength(25)] public string? Telefono { get; set; }
    [MaxLength(80)] public string? Ciudad { get; set; }
}

// ===================== Directorio =====================

public class Etiqueta
{
    [Key] public int IdEtiqueta { get; set; }
    [MaxLength(60)] public string Texto { get; set; } = "";
    [MaxLength(9)] public string ColorTexto { get; set; } = "";
    [MaxLength(9)] public string ColorFondo { get; set; } = "";
}

public class Asociacion
{
    [Key] public int IdAsociacion { get; set; }

    // Las asociaciones del directorio pueden existir sin cuenta
    public int? IdUsuario { get; set; }
    [MaxLength(120)] public string Nombre { get; set; } = "";
    public int? IdFoto { get; set; }
    public int? IdIcono { get; set; }
    [MaxLength(20)] public string? Distancia { get; set; }
    [MaxLength(300)] public string? Descripcion { get; set; }
    public string? SobreNosotros { get; set; }
    [MaxLength(200)] public string? Direccion { get; set; }
    [MaxLength(100)] public string? Horario { get; set; }
    [MaxLength(25)] public string? Telefono { get; set; }
    [MaxLength(254)] public string? EmailContacto { get; set; }
    [MaxLength(20)] public string? Cuit { get; set; }
    [MaxLength(80)] public string? Ciudad { get; set; }
    public int Rescatados { get; set; }
    public int Adoptados { get; set; }

    // Una cuenta nueva no aparece en el directorio hasta completar su perfil
    public bool Activa { get; set; } = true;
    public List<Etiqueta> Etiquetas { get; set; } = [];
    [ForeignKey(nameof(AsociacionNecesidad.IdAsociacion))] public List<AsociacionNecesidad> Necesidades { get; set; } = [];
}

public class AsociacionNecesidad
{
    [Key] public int IdNecesidad { get; set; }
    public int IdAsociacion { get; set; }
    [MaxLength(120)] public string Texto { get; set; } = "";
}

public class Veterinaria
{
    [Key] public int IdVeterinaria { get; set; }
    public int? IdUsuario { get; set; }
    [MaxLength(120)] public string Nombre { get; set; } = "";
    public int? IdFoto { get; set; }
    public int? IdIcono { get; set; }
    [MaxLength(20)] public string? Distancia { get; set; }
    [Column(TypeName = "decimal(2,1)")] public decimal Puntaje { get; set; }
    public int Resenas { get; set; }
    [MaxLength(300)] public string? Descripcion { get; set; }
    public bool Abierta24h { get; set; }
    public string? SobreNosotros { get; set; }
    [MaxLength(200)] public string? Direccion { get; set; }
    [MaxLength(100)] public string? Horario { get; set; }
    [MaxLength(25)] public string? Telefono { get; set; }
    [MaxLength(254)] public string? EmailContacto { get; set; }
    [MaxLength(20)] public string? Cuit { get; set; }
    [MaxLength(30)] public string? Matricula { get; set; }
    [MaxLength(80)] public string? Ciudad { get; set; }
    public bool Disponible { get; set; } = true;
    public List<Etiqueta> Etiquetas { get; set; } = [];
    [ForeignKey(nameof(Servicio.IdVeterinaria))] public List<Servicio> Servicios { get; set; } = [];
}

public class Servicio
{
    [Key] public int IdServicio { get; set; }
    public int IdVeterinaria { get; set; }
    [MaxLength(100)] public string Nombre { get; set; } = "";
    [Column(TypeName = "decimal(12,2)")] public decimal Precio { get; set; }
}

// ===================== Solicitudes (casos) =====================

public class Animal
{
    [Key] public int IdAnimal { get; set; }
    [MaxLength(80)] public string? Nombre { get; set; }
    [MaxLength(40)] public string? Especie { get; set; }
    [MaxLength(60)] public string? Raza { get; set; }
    [MaxLength(30)] public string? EdadAproximada { get; set; }
    [MaxLength(20)] public string? Sexo { get; set; }
    [MaxLength(100)] public string? EstadoGeneral { get; set; }
}

public class EstadoSolicitud
{
    [Key] public int IdEstado { get; set; }
    [MaxLength(30)] public string Nombre { get; set; } = "";
    [MaxLength(200)] public string? Descripcion { get; set; }
}

public class CategoriaSolicitud
{
    [Key] public int IdCategoria { get; set; }
    [MaxLength(40)] public string Nombre { get; set; } = "";
}

public class Solicitud
{
    [Key] public int IdSolicitud { get; set; }
    public int IdSolicitante { get; set; }
    [ForeignKey(nameof(IdSolicitante))] public Solicitante? Solicitante { get; set; }
    public int IdAnimal { get; set; }
    [ForeignKey(nameof(IdAnimal))] public Animal? Animal { get; set; }
    public int? IdAsociacion { get; set; }
    public int IdEstado { get; set; }
    public int IdCategoria { get; set; }
    [ForeignKey(nameof(IdCategoria))] public CategoriaSolicitud? Categoria { get; set; }
    public int? IdFoto { get; set; }
    [MaxLength(150)] public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Historia { get; set; }
    [MaxLength(200)] public string? Ubicacion { get; set; }
    [MaxLength(10)] public string? Prioridad { get; set; }

    // El WhatsApp que dejó quien pidió ayuda: puede ser distinto al de su cuenta
    [MaxLength(25)] public string? TelefonoContacto { get; set; }
    public DateTime FechaCreacion { get; set; }
    [ForeignKey(nameof(SolicitudNecesidad.IdSolicitud))] public List<SolicitudNecesidad> Necesidades { get; set; } = [];
    [ForeignKey(nameof(HistorialEstado.IdSolicitud))] public List<HistorialEstado> Historial { get; set; } = [];
}

public class SolicitudNecesidad
{
    [Key] public int IdNecesidad { get; set; }
    public int IdSolicitud { get; set; }
    [MaxLength(120)] public string Texto { get; set; } = "";
}

public class Asignacion
{
    [Key] public int IdAsignacion { get; set; }
    public int IdSolicitud { get; set; }
    public int IdVeterinaria { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public bool? Aceptada { get; set; }
    [MaxLength(300)] public string? MotivoRechazo { get; set; }
}

public class Atencion
{
    [Key] public int IdAtencion { get; set; }
    public int IdAsignacion { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? Observaciones { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal Costo { get; set; }
    public DateTime FechaAtencion { get; set; }
}

public class HistorialEstado
{
    [Key] public long IdHistorial { get; set; }
    public int IdSolicitud { get; set; }
    public int IdEstado { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaHora { get; set; }
    [MaxLength(300)] public string? Observacion { get; set; }
}

// ===================== Mi Mascota =====================

// Una mascota es un animal que tiene dueño
public class Mascota
{
    [Key] public int IdMascota { get; set; }
    public int IdAnimal { get; set; }
    [ForeignKey(nameof(IdAnimal))] public Animal? Animal { get; set; }
    public int IdUsuario { get; set; }
    public int? IdFoto { get; set; }
    [MaxLength(40)] public string? RangoPeso { get; set; }
    [MaxLength(40)] public string? EstadoPeso { get; set; }
    [MaxLength(40)] public string? EstadoVacunas { get; set; }
    [MaxLength(200)] public string? Alergias { get; set; }
    [ForeignKey(nameof(MascotaConsejo.IdMascota))] public List<MascotaConsejo> Consejos { get; set; } = [];
    [ForeignKey(nameof(MascotaGaleria.IdMascota))] public List<MascotaGaleria> Galeria { get; set; } = [];
    [ForeignKey(nameof(Recordatorio.IdMascota))] public List<Recordatorio> Recordatorios { get; set; } = [];
    [ForeignKey(nameof(RegistroMedico.IdMascota))] public List<RegistroMedico> Historial { get; set; } = [];
    [ForeignKey(nameof(Peso.IdMascota))] public List<Peso> Pesos { get; set; } = [];
}

public class MascotaConsejo
{
    [Key] public int IdConsejo { get; set; }
    public int IdMascota { get; set; }
    [MaxLength(300)] public string Texto { get; set; } = "";
}

public class MascotaGaleria
{
    public int IdMascota { get; set; }
    public int IdFoto { get; set; }
    public int Orden { get; set; }
}

public class Recordatorio
{
    [Key] public int IdRecordatorio { get; set; }
    public int IdMascota { get; set; }
    public int? IdFoto { get; set; }
    [MaxLength(120)] public string Titulo { get; set; } = "";
    [MaxLength(60)] public string? Frecuencia { get; set; }
    public DateOnly Fecha { get; set; }
    [MaxLength(9)] public string ColorTexto { get; set; } = "";
    [MaxLength(9)] public string ColorFondo { get; set; } = "";
}

public class RegistroMedico
{
    [Key] public int IdRegistro { get; set; }
    public int IdMascota { get; set; }
    public DateOnly Fecha { get; set; }
    [MaxLength(120)] public string Titulo { get; set; } = "";
    public string? Detalle { get; set; }
    [MaxLength(120)] public string? Lugar { get; set; }
    [MaxLength(4)] public string Glifo { get; set; } = "";
    [MaxLength(9)] public string Color { get; set; } = "";
}

public class Peso
{
    [Key] public int IdPeso { get; set; }
    public int IdMascota { get; set; }
    public DateOnly Fecha { get; set; }
    [Column(TypeName = "decimal(6,2)")] public decimal Kg { get; set; }
}

// ===================== Contenido =====================

public class Campana
{
    [Key] public int IdCampana { get; set; }
    public int IdAsociacion { get; set; }
    [ForeignKey(nameof(IdAsociacion))] public Asociacion? Asociacion { get; set; }
    public int? IdFoto { get; set; }
    [MaxLength(120)] public string Titulo { get; set; } = "";
    [MaxLength(300)] public string? Detalle { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal MontoRecaudado { get; set; }
    public int Porcentaje { get; set; }
}

public class Novedad
{
    [Key] public int IdNovedad { get; set; }
    public int? IdFoto { get; set; }
    [MaxLength(120)] public string Titulo { get; set; } = "";
    [MaxLength(300)] public string? Descripcion { get; set; }
    public string? Detalle { get; set; }
}

public class Foto
{
    [Key] public int IdFoto { get; set; }
    [MaxLength(64)] public string Nombre { get; set; } = "";
    [MaxLength(32)] public string ContentType { get; set; } = "";
    public byte[] Datos { get; set; } = [];
}

// ===================== Favoritos de cada usuario =====================

public class FavoritoAsociacion
{
    public int IdUsuario { get; set; }
    public int IdAsociacion { get; set; }
}

public class FavoritoVeterinaria
{
    public int IdUsuario { get; set; }
    public int IdVeterinaria { get; set; }
}

public class FavoritoSolicitud
{
    public int IdUsuario { get; set; }
    public int IdSolicitud { get; set; }
}
