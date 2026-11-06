# Guía del código de RescueVet

Explica qué hace cada parte del proyecto y cómo se conectan. Además, cada archivo de código tiene un comentario al inicio con su función.

## Cómo funciona en general

```
App (Demo)  →  API (Backend)  →  SQL Server (base RescueVet_Normalizada)
```

1. **La app** muestra las pantallas y nunca toca la base directamente.
2. **La API** recibe los pedidos, los valida y lee o escribe en la base.
3. **SQL Server** guarda datos, cuentas e imágenes. Se conecta a `.\SQLEXPRESS` con autenticación de Windows.

## Qué ve cada cuenta

| Cuenta | Pantallas |
|---|---|
| Usuario común | Inicio, Ayuda, Donación y Favoritos, más Mi Mascota y Reportar caso |
| Asociación | Casos y Configuración |
| Veterinaria | Lo mismo que el usuario común, por ahora |
| Facebook / Google | Son de muestra: entran sin cuenta, como invitado |

## Estructura de carpetas

```
Backend/                 API en ASP.NET Core
  Program.cs             Punto de entrada
  appsettings.json       Conexión a la base y puerto 5080
  Controllers/           Rutas de la API
  Data/                  Conexión a la base y datos iniciales
  Models/                Clases que representan las tablas
  ImagenesIniciales/     Imágenes para llenar una base nueva
Demo/                    App en .NET MAUI
  App y AppShell         Inicio y navegación
  Controls/              Controles reutilizables
  Models/                Datos que llegan de la API
  Services/              Conexión con la API, datos compartidos y contacto
  Views/                 Pantallas, por sección
  Resources/             Colores, estilos, fuentes e imágenes de la interfaz
docs/                    Documentación
```

## Backend

| Archivo | Qué hace |
|---|---|
| `Program.cs` | Arranca la API, conecta la base, crea los datos iniciales y activa Swagger |
| `Data/AppDbContext.cs` | Define las tablas, relaciones, índices y reglas para Entity Framework |
| `Data/Semilla.cs` | Crea la base si no existe y, si está vacía, carga catálogos, imágenes y ejemplos |
| `Data/MigrarDesdeBaseAnterior.sql` | Copia los datos de la base anterior `RescueVet` a la normalizada |
| `Models/Entidades.cs` | Clases de las tablas normalizadas |
| `Models/Dtos.cs` | Datos que entran y salen de la API |
| `Controllers/CrudController.cs` | Rutas comunes y favoritos |
| `Controllers/DirectorioController.cs` | Asociaciones y veterinarias |
| `Controllers/CasoController.cs` | Casos (solicitudes) y fotos |
| `Controllers/ConsultasController.cs` | Mascota, recordatorios, historial, pesos, campañas y novedades |
| `Controllers/UsuarioController.cs` | Registro e inicio de sesión |

### Rutas de la API

Con la API encendida, todas se pueden probar en http://localhost:5080/swagger

| Ruta | Para qué |
|---|---|
| `GET /api/{Tabla}/Lista` | Trae todos los registros |
| `GET /api/{Tabla}/Obtener/{id}` | Trae uno |
| `POST /api/{Tabla}/Guardar` | Crea uno |
| `PUT /api/{Tabla}/Editar/{id}` | Modifica uno |
| `DELETE /api/{Tabla}/Eliminar/{id}` | Elimina uno |
| `PUT /api/{Asociacion, Veterinaria o Caso}/Favorito/{id}/{true o false}?usuarioId={id}` | Marca o quita un favorito de esa cuenta |
| `POST /api/Caso/SubirFoto` | Guarda una foto en la base y devuelve su ruta |
| `GET /fotos/{nombre}` | Devuelve una imagen guardada |
| `POST /api/Usuario/Registrar` | Crea una cuenta |
| `POST /api/Usuario/Login` | Valida correo y contraseña |

`{Tabla}` puede ser `Asociacion`, `Veterinaria`, `Caso`, `Mascota`, `Recordatorio`, `RegistroMedico`, `Peso`, `Campana` o `Novedad`. `Lista` y `Obtener` aceptan `?usuarioId={id}` para indicar los favoritos de esa cuenta, y `Mascota/Lista` para traer solo sus mascotas.

### Tablas de la base

La base `RescueVet_Normalizada` tiene 30 tablas. La API sigue usando los nombres de la app (por ejemplo, un `Caso` es una `solicitud`).

| Grupo | Tablas | Relación principal |
|---|---|---|
| Cuentas | `rol`, `usuario`, `solicitante` | `rol` 1 a N `usuario`; `usuario` 1 a 0..1 `solicitante` |
| Directorio | `asociacion`, `veterinaria`, `etiqueta`, `asociacion_etiqueta`, `veterinaria_etiqueta`, `asociacion_necesidad`, `servicio` | `usuario` 1 a 0..1 `asociacion` o `veterinaria` |
| Solicitudes | `solicitud`, `animal`, `estado_solicitud`, `categoria_solicitud`, `solicitud_necesidad`, `historial_estado` | `solicitante`, `animal`, `estado_solicitud` y `asociacion` 1 a N `solicitud` |
| Atención | `asignacion`, `atencion` | `solicitud` y `veterinaria` 1 a N `asignacion`; `asignacion` 1 a 0..1 `atencion` |
| Mi Mascota | `mascota`, `mascota_consejo`, `mascota_galeria`, `recordatorio`, `registro_medico`, `peso` | `usuario` 1 a N `mascota`; `mascota` 1 a 1 `animal` |
| Contenido | `campana`, `novedad`, `foto` | `asociacion` 1 a N `campana`; las imágenes se referencian por `id_foto` |
| Favoritos | `favorito_asociacion`, `favorito_veterinaria`, `favorito_solicitud` | Una fila por cuenta y elemento |

`asignacion` y `atencion` ya existen, pero la app todavía no las usa. Quien publica sin cuenta queda como solicitante de una cuenta sin acceso (`activo = 0`).

## App móvil

### Servicios

| Archivo | Qué hace |
|---|---|
| `Services/Api.cs` | Todas las llamadas al backend y la cuenta actual. Aquí se cambia la IP (`IpPc`) |
| `Services/Datos.cs` | Listas que comparten las pantallas; se cargan al iniciar sesión |
| `Services/Contacto.cs` | Abre WhatsApp y el mapa |

### Controles reutilizables

| Control | Qué es |
|---|---|
| `PageHeader` | Cabecera con título, subtítulo e ilustración |
| `BotonAtras` | Botón redondo para volver |
| `CampoFormulario` | Campo de texto con etiqueta, ícono y ayuda |
| `FilaInfo` | Fila con ícono, título y valor |

### Pantallas

| Carpeta | Pantallas |
|---|---|
| `Views/Auth` | `LoginPage`, `RegistroPage` |
| `Views/Inicio` | `HomePage`, `ConfiguracionPage` |
| `Views/Veterinarias` | `VeterinariasPage`, `VeterinariaDetallePage` |
| `Views/Asociaciones` | `AsociacionesPage`, `AsociacionDetallePage` |
| `Views/Casos` | `CasosPage`, `CasoDetallePage`, `ReportarCasoPage` |
| `Views/MiMascota` | `MiMascotaPage`, `RecordatoriosPage`, `HistorialMedicoPage`, `CrecimientoPage`, `FormularioPage` |
| `Views/Favoritos` | `FavoritosPage` |
| `Views/Donaciones` | `DonacionesPage` |
| `Views/Emergencia` | `EmergenciaPage` |

Cada pantalla tiene dos archivos: `.xaml` define lo que se ve y `.xaml.cs` lo que hace al tocarla.

## Recorridos principales

**Iniciar sesión**
1. `LoginPage` envía correo y contraseña con `Api.Login`.
2. `UsuarioController` compara el hash.
3. `Datos.CargarAsync` trae los datos.
4. La app abre las pestañas de usuario o el panel de asociación.

**Reportar un caso con foto**
1. `ReportarCasoPage` elige la foto y valida el formulario.
2. `Api.SubirFoto` la guarda en la tabla `Foto` y recibe su ruta.
3. `Api.GuardarCaso` guarda el caso con esa ruta.
4. El usuario ve "¡Publicado!", y la asociación lo verá al volver a iniciar sesión.

**Mostrar una imagen**
1. Las tablas guardan un `id_foto`, y la API lo devuelve como una ruta, como `fotos/rv_caso_luna.png`.
2. `FotoUrl` la convierte en dirección completa con `Api.UrlFoto`.
3. La API la busca en la tabla `Foto` y la devuelve.

**Marcar un favorito**
1. La pantalla llama a `Datos.AlternarFavorito`.
2. La API guarda el cambio para esa cuenta en la base.
3. El corazón y la lista de Favoritos se actualizan.

## Dónde cambiar cosas comunes

| Quiero cambiar | Dónde |
|---|---|
| La IP para usar otro Wi-Fi | `IpPc` en `Demo/Services/Api.cs` |
| La conexión a la base o el puerto | `Backend/appsettings.json` |
| El código de país de WhatsApp | `CodigoPais` en `Demo/Services/Contacto.cs` |
| Los datos de ejemplo para una base nueva | `Backend/Data/Semilla.cs` |
| Colores y estilos | `Demo/Resources/Styles` |
| Las pestañas | `Demo/AppShell.xaml` |
| Una ruta o validación de la API | `Backend/Controllers` |
| El número de la línea de emergencia | `LineaEmergencia` en `EmergenciaPage.xaml.cs` (hoy es de prueba) |

## Limitaciones actuales

- **Seguridad:** la API no exige login ni tokens. Hay que protegerla antes de publicarla.
- **Solo en la app:** reglas como "el historial es de solo lectura" o "los casos son para asociaciones" no las hace cumplir la API.
- **Cuentas sin verificar:** la API recibe el `usuarioId` sin comprobar quién lo envía, hasta que tenga login con token.
- **Perfiles de organización:** una cuenta nueva de asociación o veterinaria no aparece en el directorio hasta activar su perfil, y la app todavía no permite completarlo.
- **Sin terminar:** los filtros de casos, las preferencias, Facebook, Google y los pagos de donaciones son de muestra.
- **Datos de ejemplo:** los teléfonos de veterinarias y asociaciones son inventados.
- **Sin pruebas automáticas:** el proyecto todavía no tiene pruebas.
