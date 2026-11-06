-- Copia los datos de la base anterior (RescueVet) a la base normalizada (RescueVet_Normalizada).
-- La base anterior solo se lee: no se modifica. Todo se hace en una transacción: se copia completo o no se copia nada.
-- Requisito: crear antes las tablas vacías con: dotnet run --no-launch-profile --SinSemilla=true
SET NOCOUNT ON;
SET XACT_ABORT ON;
USE RescueVet_Normalizada;
GO
IF EXISTS (SELECT 1 FROM rol) THROW 50000, 'La base normalizada ya tiene datos.', 1;
GO
BEGIN TRANSACTION;

-- ===== Catálogos =====
INSERT INTO rol (nombre) VALUES (N'Usuario'), (N'Asociacion'), (N'Veterinaria');
INSERT INTO estado_solicitud (nombre, descripcion) VALUES
 (N'Pendiente', N'Publicada, a la espera de una asociación'), (N'Aprobada', N'Una asociación tomó el caso'),
 (N'Rechazada', N'La solicitud no se pudo tomar'), (N'Derivada', N'Se asignó una veterinaria'),
 (N'En atencion', N'La veterinaria atiende al animal'), (N'Cerrada', N'El caso terminó');
INSERT INTO categoria_solicitud (nombre) SELECT DISTINCT Categoria FROM RescueVet.dbo.Caso;

-- ===== Fotos =====
SET IDENTITY_INSERT foto ON;
INSERT INTO foto (id_foto, nombre, content_type, datos) SELECT Id, Nombre, ContentType, Datos FROM RescueVet.dbo.Foto;
SET IDENTITY_INSERT foto OFF;

-- ===== Cuentas (conservan su id y su contraseña) =====
SET IDENTITY_INSERT usuario ON;
INSERT INTO usuario (id_usuario, id_rol, email, password_hash, activo, fecha_registro)
SELECT u.Id, r.id_rol, u.Correo, u.ClaveHash, 1, SYSUTCDATETIME() FROM RescueVet.dbo.Usuario u JOIN rol r ON r.nombre = u.Tipo;
SET IDENTITY_INSERT usuario OFF;

INSERT INTO solicitante (id_usuario, nombre, apellido, documento, telefono, ciudad)
SELECT Id, Nombre, NULLIF(Apellido,''), NULLIF(Documento,''), NULLIF(Telefono,''), NULLIF(Ciudad,'') FROM RescueVet.dbo.Usuario WHERE Tipo = 'Usuario';

-- ===== Directorio (conserva sus id) =====
SET IDENTITY_INSERT asociacion ON;
INSERT INTO asociacion (id_asociacion, nombre, id_foto, id_icono, distancia, descripcion, sobre_nosotros, direccion, horario, telefono, email_contacto, rescatados, adoptados, activa)
SELECT a.Id, a.Nombre, f.Id, i.Id, a.Distancia, a.Descripcion, a.SobreNosotros, a.Direccion, a.Horario, a.Telefono, a.Email, a.Rescatados, a.Adoptados, 1
FROM RescueVet.dbo.Asociacion a
LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = a.Foto LEFT JOIN RescueVet.dbo.Foto i ON 'fotos/' + i.Nombre = a.Icono;
SET IDENTITY_INSERT asociacion OFF;

SET IDENTITY_INSERT veterinaria ON;
INSERT INTO veterinaria (id_veterinaria, nombre, id_foto, id_icono, distancia, puntaje, resenas, descripcion, abierta_24h, sobre_nosotros, direccion, horario, telefono, disponible)
SELECT v.Id, v.Nombre, f.Id, i.Id, v.Distancia, v.Puntaje, v.Resenas, v.Descripcion, v.Abierta24h, v.SobreNosotros, v.Direccion, v.Horario, v.Telefono, 1
FROM RescueVet.dbo.Veterinaria v
LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = v.Foto LEFT JOIN RescueVet.dbo.Foto i ON 'fotos/' + i.Nombre = v.Icono;
SET IDENTITY_INSERT veterinaria OFF;

-- Las cuentas de asociación o veterinaria tienen su perfil, sin aparecer todavía en el directorio
INSERT INTO asociacion (id_usuario, nombre, cuit, telefono, ciudad, email_contacto, rescatados, adoptados, activa)
SELECT Id, Nombre, NULLIF(Documento,''), NULLIF(Telefono,''), NULLIF(Ciudad,''), Correo, 0, 0, 0 FROM RescueVet.dbo.Usuario WHERE Tipo = 'Asociacion';
INSERT INTO veterinaria (id_usuario, nombre, cuit, matricula, telefono, ciudad, email_contacto, puntaje, resenas, abierta_24h, disponible)
SELECT Id, Nombre, NULLIF(Documento,''), NULLIF(Matricula,''), NULLIF(Telefono,''), NULLIF(Ciudad,''), Correo, 0, 0, 0, 0 FROM RescueVet.dbo.Usuario WHERE Tipo = 'Veterinaria';

-- Etiquetas sin repetir, y su relación N a N
INSERT INTO etiqueta (texto, color_texto, color_fondo)
SELECT DISTINCT e.Texto, e.ColorTexto, e.ColorFondo FROM (
  SELECT Etiquetas FROM RescueVet.dbo.Asociacion UNION ALL SELECT Etiquetas FROM RescueVet.dbo.Veterinaria) x
CROSS APPLY OPENJSON(x.Etiquetas) WITH (Texto nvarchar(60), ColorTexto nvarchar(9), ColorFondo nvarchar(9)) e;

INSERT INTO asociacion_etiqueta (id_asociacion, id_etiqueta)
SELECT a.Id, t.id_etiqueta FROM RescueVet.dbo.Asociacion a
CROSS APPLY OPENJSON(a.Etiquetas) WITH (Texto nvarchar(60), ColorTexto nvarchar(9), ColorFondo nvarchar(9)) e
JOIN etiqueta t ON t.texto = e.Texto AND t.color_texto = e.ColorTexto AND t.color_fondo = e.ColorFondo;

INSERT INTO veterinaria_etiqueta (id_veterinaria, id_etiqueta)
SELECT v.Id, t.id_etiqueta FROM RescueVet.dbo.Veterinaria v
CROSS APPLY OPENJSON(v.Etiquetas) WITH (Texto nvarchar(60), ColorTexto nvarchar(9), ColorFondo nvarchar(9)) e
JOIN etiqueta t ON t.texto = e.Texto AND t.color_texto = e.ColorTexto AND t.color_fondo = e.ColorFondo;

INSERT INTO asociacion_necesidad (id_asociacion, texto)
SELECT a.Id, j.value FROM RescueVet.dbo.Asociacion a CROSS APPLY OPENJSON(a.Necesidades) j ORDER BY a.Id, CAST(j.[key] AS int);

-- Los precios pasan de texto ("$8.000") a número
INSERT INTO servicio (id_veterinaria, nombre, precio)
SELECT v.Id, s.Nombre, CAST(REPLACE(REPLACE(s.Precio,'$',''),'.','') AS decimal(12,2))
FROM RescueVet.dbo.Veterinaria v CROSS APPLY OPENJSON(v.Servicios) WITH (Nombre nvarchar(100), Precio nvarchar(30)) s;

-- ===== Solicitantes de casos sin cuenta: cuentas sin acceso =====
CREATE TABLE #publica (nombre nvarchar(80) PRIMARY KEY, email nvarchar(254) NULL);
INSERT INTO #publica (nombre) SELECT DISTINCT PublicadoPor FROM RescueVet.dbo.Caso
  WHERE PublicadoPor NOT IN (SELECT s.nombre FROM solicitante s);
IF NOT EXISTS (SELECT 1 FROM #publica WHERE nombre = N'Anónimo') INSERT INTO #publica (nombre) VALUES (N'Anónimo');

;WITH P AS (SELECT nombre, email, ROW_NUMBER() OVER (ORDER BY nombre) n FROM #publica)
UPDATE P SET email = CASE WHEN nombre = N'Anónimo' THEN N'anonimo@rescuevet.invalid'
                          ELSE CONCAT(N'sin-cuenta-', n, N'@rescuevet.invalid') END;

DECLARE @rolUsuario int = (SELECT id_rol FROM rol WHERE nombre = N'Usuario');
INSERT INTO usuario (id_rol, email, password_hash, activo, fecha_registro)
SELECT @rolUsuario, email, N'', 0, SYSUTCDATETIME() FROM #publica;

INSERT INTO solicitante (id_usuario, nombre)
SELECT u.id_usuario, p.nombre FROM #publica p JOIN usuario u ON u.email = p.email;

-- ===== Solicitudes: cada caso genera su animal, solicitud, necesidades e historial =====
CREATE TABLE #animal (id_caso int PRIMARY KEY, id_animal int);
MERGE animal AS t USING (SELECT Id, NULLIF(Especie,'') Especie FROM RescueVet.dbo.Caso) AS s ON 1 = 0
WHEN NOT MATCHED THEN INSERT (especie) VALUES (s.Especie)
OUTPUT s.Id, inserted.id_animal INTO #animal;

DECLARE @pendiente int = (SELECT id_estado FROM estado_solicitud WHERE nombre = N'Pendiente');
SET IDENTITY_INSERT solicitud ON;
INSERT INTO solicitud (id_solicitud, id_solicitante, id_animal, id_estado, id_categoria, id_foto, titulo, descripcion, historia, ubicacion, prioridad, telefono_contacto, fecha_creacion)
SELECT c.Id, s.id_solicitante, a.id_animal, @pendiente, k.id_categoria, f.Id, c.Nombre, c.Descripcion, c.Historia, NULLIF(c.Ubicacion,''), NULLIF(c.Prioridad,''), NULLIF(c.Telefono,''), c.Fecha
FROM RescueVet.dbo.Caso c
JOIN #animal a ON a.id_caso = c.Id
JOIN solicitante s ON s.nombre = c.PublicadoPor
JOIN categoria_solicitud k ON k.nombre = c.Categoria
LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = c.Foto;
SET IDENTITY_INSERT solicitud OFF;

INSERT INTO solicitud_necesidad (id_solicitud, texto)
SELECT c.Id, j.value FROM RescueVet.dbo.Caso c CROSS APPLY OPENJSON(c.Necesita) j ORDER BY c.Id, CAST(j.[key] AS int);

INSERT INTO historial_estado (id_solicitud, id_estado, id_usuario, fecha_hora, observacion)
SELECT s.id_solicitud, @pendiente, p.id_usuario, s.fecha_creacion, N'Caso publicado'
FROM solicitud s JOIN solicitante p ON p.id_solicitante = s.id_solicitante;

-- ===== Mi Mascota: pertenece a la primera cuenta común =====
DECLARE @duenio int = (SELECT MIN(Id) FROM RescueVet.dbo.Usuario WHERE Tipo = 'Usuario');
CREATE TABLE #animalMascota (id_mascota int PRIMARY KEY, id_animal int);
MERGE animal AS t USING (SELECT Id, Nombre, Raza, Edad, Sexo FROM RescueVet.dbo.Mascota) AS s ON 1 = 0
WHEN NOT MATCHED THEN INSERT (nombre, raza, edad_aproximada, sexo) VALUES (s.Nombre, s.Raza, s.Edad, s.Sexo)
OUTPUT s.Id, inserted.id_animal INTO #animalMascota;

SET IDENTITY_INSERT mascota ON;
INSERT INTO mascota (id_mascota, id_animal, id_usuario, id_foto, rango_peso, estado_peso, estado_vacunas, alergias)
SELECT m.Id, a.id_animal, @duenio, f.Id, m.RangoPeso, m.EstadoPeso, m.EstadoVacunas, m.Alergias
FROM RescueVet.dbo.Mascota m JOIN #animalMascota a ON a.id_mascota = m.Id
LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = m.Foto;
SET IDENTITY_INSERT mascota OFF;

INSERT INTO mascota_consejo (id_mascota, texto)
SELECT m.Id, j.value FROM RescueVet.dbo.Mascota m CROSS APPLY OPENJSON(m.Consejos) j ORDER BY m.Id, CAST(j.[key] AS int);

INSERT INTO mascota_galeria (id_mascota, id_foto, orden)
SELECT m.Id, f.Id, CAST(j.[key] AS int) + 1 FROM RescueVet.dbo.Mascota m CROSS APPLY OPENJSON(m.Galeria) j
JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = j.value;

SET IDENTITY_INSERT recordatorio ON;
INSERT INTO recordatorio (id_recordatorio, id_mascota, id_foto, titulo, frecuencia, fecha, color_texto, color_fondo)
SELECT r.Id, r.MascotaId, f.Id, r.Titulo, r.Frecuencia, r.Fecha, r.ColorTexto, r.ColorFondo
FROM RescueVet.dbo.Recordatorio r LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = r.Imagen;
SET IDENTITY_INSERT recordatorio OFF;

SET IDENTITY_INSERT registro_medico ON;
INSERT INTO registro_medico (id_registro, id_mascota, fecha, titulo, detalle, lugar, glifo, color)
SELECT Id, MascotaId, Fecha, Titulo, Detalle, Lugar, Glifo, Color FROM RescueVet.dbo.RegistroMedico;
SET IDENTITY_INSERT registro_medico OFF;

SET IDENTITY_INSERT peso ON;
INSERT INTO peso (id_peso, id_mascota, fecha, kg) SELECT Id, MascotaId, Fecha, Kg FROM RescueVet.dbo.Peso;
SET IDENTITY_INSERT peso OFF;

-- ===== Contenido =====
SET IDENTITY_INSERT campana ON;
INSERT INTO campana (id_campana, id_asociacion, id_foto, titulo, detalle, monto_recaudado, porcentaje)
SELECT c.Id, a.id_asociacion, f.Id, c.Titulo, c.Detalle,
       CAST(REPLACE(REPLACE(REPLACE(c.Recaudado,'$',''),'.',''),' recaudados','') AS decimal(12,2)), c.Porcentaje
FROM RescueVet.dbo.Campana c JOIN asociacion a ON a.nombre = c.Asociacion AND a.activa = 1
LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = c.Foto;
SET IDENTITY_INSERT campana OFF;

SET IDENTITY_INSERT novedad ON;
INSERT INTO novedad (id_novedad, id_foto, titulo, descripcion, detalle)
SELECT n.Id, f.Id, n.Titulo, n.Descripcion, n.Detalle FROM RescueVet.dbo.Novedad n LEFT JOIN RescueVet.dbo.Foto f ON 'fotos/' + f.Nombre = n.Foto;
SET IDENTITY_INSERT novedad OFF;

-- ===== Favoritos: eran globales; pasan a la cuenta que puede verlos =====
INSERT INTO favorito_asociacion (id_usuario, id_asociacion) SELECT @duenio, Id FROM RescueVet.dbo.Asociacion WHERE EsFavorito = 1 AND @duenio IS NOT NULL;
INSERT INTO favorito_veterinaria (id_usuario, id_veterinaria) SELECT @duenio, Id FROM RescueVet.dbo.Veterinaria WHERE EsFavorito = 1 AND @duenio IS NOT NULL;
DECLARE @cuentaAsociacion int = (SELECT MIN(Id) FROM RescueVet.dbo.Usuario WHERE Tipo = 'Asociacion');
INSERT INTO favorito_solicitud (id_usuario, id_solicitud) SELECT @cuentaAsociacion, Id FROM RescueVet.dbo.Caso WHERE EsFavorito = 1 AND @cuentaAsociacion IS NOT NULL;

COMMIT;
PRINT 'Migración terminada.';
GO
