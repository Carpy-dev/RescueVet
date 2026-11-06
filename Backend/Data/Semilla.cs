// Datos iniciales: crea la base si no existe y, si está vacía, carga catálogos, imágenes y ejemplos.
// Con el parámetro --SinSemilla=true solo crea las tablas (lo usa la migración desde la base anterior).
using RescueVet.Api.Models;

namespace RescueVet.Api.Data;

public static class Semilla
{
    private static readonly (string Texto, string Fondo) Violeta = ("#744DEF", "#F0EAFD");
    private static readonly (string Texto, string Fondo) Verde = ("#1FA463", "#E6FBEE");
    private static readonly (string Texto, string Fondo) Ambar = ("#E39A0E", "#FFF3DE");

    public static void Cargar(AppDbContext db, bool sinSemilla)
    {
        db.Database.EnsureCreated();
        if (sinSemilla || db.Roles.Any())
            return;

        CargarCatalogos(db);
        var fotos = CargarImagenes(db);
        CargarDirectorio(db, fotos);
        CargarCasos(db, fotos);
        CargarContenido(db, fotos);
    }

    private static void CargarCatalogos(AppDbContext db)
    {
        db.Roles.AddRange(new Rol { Nombre = "Usuario" }, new Rol { Nombre = "Asociacion" }, new Rol { Nombre = "Veterinaria" });
        db.EstadosSolicitud.AddRange(
            new EstadoSolicitud { Nombre = "Pendiente", Descripcion = "Publicada, a la espera de una asociación" },
            new EstadoSolicitud { Nombre = "Aprobada", Descripcion = "Una asociación tomó el caso" },
            new EstadoSolicitud { Nombre = "Rechazada", Descripcion = "La solicitud no se pudo tomar" },
            new EstadoSolicitud { Nombre = "Derivada", Descripcion = "Se asignó una veterinaria" },
            new EstadoSolicitud { Nombre = "En atencion", Descripcion = "La veterinaria atiende al animal" },
            new EstadoSolicitud { Nombre = "Cerrada", Descripcion = "El caso terminó" });
        db.CategoriasSolicitud.AddRange(new CategoriaSolicitud { Nombre = "Rescate" }, new CategoriaSolicitud { Nombre = "Médico" }, new CategoriaSolicitud { Nombre = "Alimentación" });
        db.SaveChanges();
    }

    private static Dictionary<string, Foto> CargarImagenes(AppDbContext db)
    {
        string carpeta = Path.Combine(AppContext.BaseDirectory, "ImagenesIniciales");
        var fotos = new Dictionary<string, Foto>();
        if (Directory.Exists(carpeta))
            foreach (string archivo in Directory.GetFiles(carpeta, "*.png"))
            {
                var foto = new Foto { Nombre = Path.GetFileName(archivo), ContentType = "image/png", Datos = File.ReadAllBytes(archivo) };
                fotos[foto.Nombre] = foto;
                db.Fotos.Add(foto);
            }
        db.SaveChanges();
        return fotos;
    }

    private static int? Id(Dictionary<string, Foto> fotos, string nombre) => fotos.TryGetValue(nombre, out var f) ? f.IdFoto : null;

    private static Etiqueta Tag(Dictionary<string, Etiqueta> etiquetas, string texto, (string Texto, string Fondo) color)
    {
        string clave = texto + color.Texto;
        if (!etiquetas.TryGetValue(clave, out var etiqueta))
            etiquetas[clave] = etiqueta = new Etiqueta { Texto = texto, ColorTexto = color.Texto, ColorFondo = color.Fondo };
        return etiqueta;
    }

    private static List<AsociacionNecesidad> Necesidades(params string[] textos) => textos.Select(t => new AsociacionNecesidad { Texto = t }).ToList();

    private static void CargarDirectorio(AppDbContext db, Dictionary<string, Foto> f)
    {
        var e = new Dictionary<string, Etiqueta>();

        db.Asociaciones.AddRange(
            new Asociacion
            {
                Nombre = "Patitas Felices", Distancia = "2.4 km", IdFoto = Id(f, "rv_asoc_patitas.png"), IdIcono = Id(f, "rv_asoc_ic1.png"),
                Descripcion = "Rescate y rehabilitación de perros en situación de calle.",
                SobreNosotros = "Somos un grupo que desde 2015 rescata perros abandonados, los rehabilita física y emocionalmente y les busca una familia responsable.",
                Direccion = "Av. San Martín 1240", Horario = "Lun a Sáb · 9 a 18 h", Telefono = "1145550101", EmailContacto = "hola@patitasfelices.org",
                Rescatados = 320, Adoptados = 254,
                Etiquetas = [Tag(e, "Rescate", Violeta), Tag(e, "Rehabilitación", Violeta)],
                Necesidades = Necesidades("Alimento balanceado", "Mantas y camas", "Hogares de tránsito"),
            },
            new Asociacion
            {
                Nombre = "Huellitas de Amor", Distancia = "4.1 km", IdFoto = Id(f, "rv_asoc_huellitas.png"), IdIcono = Id(f, "rv_asoc_ic2.png"),
                Descripcion = "Asociación dedicada al rescate y adopción de gatos.",
                SobreNosotros = "Rescatamos gatos de colonias urbanas, los castramos, vacunamos y acompañamos cada adopción con seguimiento.",
                Direccion = "Belgrano 455", Horario = "Mar a Dom · 10 a 17 h", Telefono = "1145550202", EmailContacto = "contacto@huellitasdeamor.org",
                Rescatados = 410, Adoptados = 362,
                Etiquetas = [Tag(e, "Adopción", Verde), Tag(e, "Rescate", Verde)],
                Necesidades = Necesidades("Arena sanitaria", "Alimento para gatitos", "Castraciones"),
            },
            new Asociacion
            {
                Nombre = "Un Hogar", Distancia = "5.7 km", IdFoto = Id(f, "rv_masc_gal3.png"), IdIcono = Id(f, "rv_asoc_ic3.png"),
                Descripcion = "Brindamos apoyo, alimento y cuidado a animales rescatados.",
                SobreNosotros = "Funcionamos como refugio temporal: damos techo, comida y atención veterinaria hasta que cada animal encuentra un hogar.",
                Direccion = "Ruta 8 km 52", Horario = "Todos los días · 8 a 20 h", Telefono = "1145550303", EmailContacto = "info@unhogar.org",
                Rescatados = 185, Adoptados = 120,
                Etiquetas = [Tag(e, "Apoyo", Ambar)],
                Necesidades = Necesidades("Medicamentos"),
            });

        db.Veterinarias.AddRange(
            new Veterinaria
            {
                Nombre = "VetSalud Centro", Distancia = "2.1 km", Puntaje = 4.8m, Resenas = 128, IdFoto = Id(f, "rv_vet_vetsalud.png"), IdIcono = Id(f, "rv_vet_ic1.png"), Abierta24h = true,
                Descripcion = "Atención integral para tus mascotas 24/7.",
                SobreNosotros = "Clínica con más de 20 años de experiencia. Guardia permanente, internación y plan de vacunación para perros y gatos.",
                Direccion = "Rivadavia 3020", Horario = "Abierto las 24 horas", Telefono = "1145551010",
                Etiquetas = [Tag(e, "Medicina general", Violeta), Tag(e, "Vacunación", Violeta)],
                Servicios = [new() { Nombre = "Consulta general", Precio = 8000 }, new() { Nombre = "Vacunación", Precio = 6500 }, new() { Nombre = "Guardia nocturna", Precio = 12000 }],
            },
            new Veterinaria
            {
                Nombre = "Patitas Felices", Distancia = "3.4 km", Puntaje = 4.6m, Resenas = 95, IdFoto = Id(f, "rv_vet_patitas.png"), IdIcono = Id(f, "rv_vet_ic2.png"),
                Descripcion = "Clínica con especialistas y laboratorio propio.",
                SobreNosotros = "Contamos con dermatología, cardiología y laboratorio propio con resultados en el día.",
                Direccion = "Mitre 870", Horario = "Lun a Sáb · 9 a 20 h", Telefono = "1145552020",
                Etiquetas = [Tag(e, "Consultas", Verde), Tag(e, "Laboratorio", Verde)],
                Servicios = [new() { Nombre = "Consulta general", Precio = 7500 }, new() { Nombre = "Análisis de sangre", Precio = 9000 }, new() { Nombre = "Dermatología", Precio = 10500 }],
            },
            new Veterinaria
            {
                Nombre = "Animed Hospital", Distancia = "4.2 km", Puntaje = 4.7m, Resenas = 142, IdFoto = Id(f, "rv_vet_animed.png"), IdIcono = Id(f, "rv_vet_ic3.png"), Abierta24h = true,
                Descripcion = "Hospital veterinario especializado 24 horas.",
                SobreNosotros = "Hospital de alta complejidad: quirófanos, terapia intensiva y diagnóstico por imágenes.",
                Direccion = "Av. Libertador 5100", Horario = "Abierto las 24 horas", Telefono = "1145553030",
                Etiquetas = [Tag(e, "Urgencias", Ambar), Tag(e, "Cirugías", Ambar)],
                Servicios = [new() { Nombre = "Urgencias", Precio = 15000 }, new() { Nombre = "Cirugía (desde)", Precio = 60000 }, new() { Nombre = "Ecografía", Precio = 14000 }],
            });
        db.SaveChanges();
    }

    // Quien publica sin una cuenta real queda como solicitante de una cuenta sin acceso
    private static Solicitante SinCuenta(AppDbContext db, string nombre, string correo)
    {
        var rol = db.Roles.First(r => r.Nombre == "Usuario");
        var usuario = new Usuario { IdRol = rol.IdRol, Email = correo, PasswordHash = "", Activo = false, FechaRegistro = DateTime.UtcNow };
        var solicitante = new Solicitante { Usuario = usuario, Nombre = nombre };
        db.Solicitantes.Add(solicitante);
        db.SaveChanges();
        return solicitante;
    }

    private static void CargarCasos(AppDbContext db, Dictionary<string, Foto> f)
    {
        SinCuenta(db, "Anónimo", "anonimo@rescuevet.invalid");
        var pendiente = db.EstadosSolicitud.First(e => e.Nombre == "Pendiente");
        var ahora = DateTime.UtcNow;

        void Caso(string publica, string correo, string categoria, string titulo, string ubicacion, int horas, string foto, string descripcion, string historia, string telefono, params string[] necesita)
        {
            var solicitante = SinCuenta(db, publica, correo);
            var fecha = ahora.AddHours(-horas);
            db.Solicitudes.Add(new Solicitud
            {
                IdSolicitante = solicitante.IdSolicitante,
                Animal = new Animal(),
                IdEstado = pendiente.IdEstado,
                IdCategoria = db.CategoriasSolicitud.First(c => c.Nombre == categoria).IdCategoria,
                IdFoto = Id(f, foto),
                Titulo = titulo, Ubicacion = ubicacion, Descripcion = descripcion, Historia = historia, TelefonoContacto = telefono, FechaCreacion = fecha,
                Necesidades = necesita.Select(t => new SolicitudNecesidad { Texto = t }).ToList(),
                Historial = [new HistorialEstado { IdEstado = pendiente.IdEstado, IdUsuario = solicitante.IdUsuario, FechaHora = fecha, Observacion = "Caso publicado" }],
            });
            db.SaveChanges();
        }

        Caso("María G.", "maria.g@rescuevet.invalid", "Rescate", "Cachorrito necesita hogar", "Guadalajara, Jalisco", 1, "rv_caso_cachorro.png",
            "Encontré este cachorrito en la calle, está mojado y asustado. Necesita un hogar temporal o definitivo.",
            "Lo encontré bajo la lluvia cerca de la plaza principal. Tiene unos 2 meses, está sano pero muy asustado. Puedo cuidarlo unos días, pero necesita un hogar definitivo.",
            "1145559001", "Hogar de tránsito", "Adopción", "Vacunas");
        Caso("Huellitas de Amor", "huellitas@rescuevet.invalid", "Médico", "Ayuda para operación de Luna", "Monterrey, Nuevo León", 3, "rv_caso_luna.png",
            "Luna necesita una cirugía de emergencia. Cualquier apoyo económico o información es bienvenido.",
            "Luna fue atropellada y tiene una fractura de cadera. La veterinaria ya la estabilizó, pero necesita cirugía en los próximos días.",
            "1145550202", "Donación para cirugía", "Hogar para la recuperación");
        Caso("Un Hogar", "unhogar@rescuevet.invalid", "Alimentación", "Necesitamos alimento", "Puebla, Puebla", 5, "rv_caso_alimento.png",
            "Rescatamos 12 perritos de la calle y necesitamos croquetas para poder alimentarlos.",
            "Tras un operativo de rescate recibimos 12 perros adultos. Consumen unos 6 kg de alimento por día y nuestras reservas alcanzan para una semana.",
            "1145550303", "Alimento balanceado", "Platos y bebederos");
        Caso("Javier R.", "javier.r@rescuevet.invalid", "Rescate", "Gatito en peligro", "Mérida, Yucatán", 6, "rv_caso_gatito.png",
            "Este gatito está en una zona peligrosa con mucho tráfico. Necesitamos quien pueda rescatarlo.",
            "Está en el cantero central de una avenida muy transitada desde ayer. Se esconde cuando alguien se acerca; hace falta una trampa o jaula.",
            "1145559004", "Rescatista con jaula", "Hogar de tránsito");
    }

    private static void CargarContenido(AppDbContext db, Dictionary<string, Foto> f)
    {
        int Asoc(string nombre) => db.Asociaciones.First(a => a.Nombre == nombre).IdAsociacion;

        db.Campanas.AddRange(
            new Campana { Titulo = "Rescate de 5 cachorros", Detalle = "Patitas Felices · Alimentación, medicamentos y atención veterinaria.", IdAsociacion = Asoc("Patitas Felices"), IdFoto = Id(f, "rv_caso_cachorro.png"), MontoRecaudado = 7200, Porcentaje = 72 },
            new Campana { Titulo = "Cirugía de Luna", Detalle = "Huellitas de Amor · Operación de emergencia y recuperación.", IdAsociacion = Asoc("Huellitas de Amor"), IdFoto = Id(f, "rv_caso_luna.png"), MontoRecaudado = 4500, Porcentaje = 45 },
            new Campana { Titulo = "Alimento para 12 rescatados", Detalle = "Un Hogar · Croquetas para los perritos rescatados de la calle.", IdAsociacion = Asoc("Un Hogar"), IdFoto = Id(f, "rv_caso_alimento.png"), MontoRecaudado = 3200, Porcentaje = 32 });
        db.Novedades.Add(new Novedad
        {
            Titulo = "Campaña de vacunación gratuita",
            Descripcion = "Este sábado, vacunación gratuita para perros y gatos. ¡Conoce los puntos!",
            Detalle = "Este sábado de 9 a 14 h. Llevá la libreta sanitaria de tu mascota. 🐶🐱",
            IdFoto = Id(f, "rv_home_novedad.png"),
        });
        db.SaveChanges();
    }
}
