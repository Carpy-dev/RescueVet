// Cuentas de usuario: registro e inicio de sesión en /api/Usuario.
// La contraseña nunca se guarda: se guarda una sal y un hash PBKDF2. No genera tokens: la API no exige login para el resto de sus rutas.
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;
using RescueVet.Api.Models;

namespace RescueVet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController(AppDbContext db) : ControllerBase
{
    private const int Iteraciones = 100_000;

    [HttpPost("Registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroRequest datos)
    {
        string correo = datos.Correo?.Trim().ToLowerInvariant() ?? "";
        string tipo = string.IsNullOrWhiteSpace(datos.Tipo) ? "Usuario" : datos.Tipo.Trim();
        string? documento = Limpio(datos.Documento);

        if (string.IsNullOrWhiteSpace(datos.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio." });
        if (!correo.Contains('@'))
            return BadRequest(new { mensaje = "Ingresá un correo válido." });
        if (datos.Clave is null || datos.Clave.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == tipo);
        if (rol is null)
            return BadRequest(new { mensaje = "El tipo de cuenta no es válido." });
        if (await db.Usuarios.AnyAsync(u => u.Email == correo))
            return Conflict(new { mensaje = "Ya existe una cuenta con ese correo." });
        if (tipo == "Usuario" && documento is not null && await db.Solicitantes.AnyAsync(s => s.Documento == documento))
            return Conflict(new { mensaje = "Ya existe una cuenta con ese documento." });

        var usuario = new Usuario { IdRol = rol.IdRol, Rol = rol, Email = correo, PasswordHash = Hashear(datos.Clave), FechaRegistro = DateTime.UtcNow };
        string nombre = datos.Nombre.Trim();
        string? telefono = Limpio(datos.Telefono), ciudad = Limpio(datos.Ciudad);

        // Cada cuenta tiene el perfil de su tipo: usuario 1 a 0..1 perfil
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        if (tipo == "Asociacion")
            db.Asociaciones.Add(new Asociacion { IdUsuario = usuario.IdUsuario, Nombre = nombre, Cuit = documento, Telefono = telefono, Ciudad = ciudad, EmailContacto = correo, Activa = false });
        else if (tipo == "Veterinaria")
            db.Veterinarias.Add(new Veterinaria { IdUsuario = usuario.IdUsuario, Nombre = nombre, Cuit = documento, Matricula = Limpio(datos.Matricula), Telefono = telefono, Ciudad = ciudad, EmailContacto = correo, Disponible = false });
        else
            db.Solicitantes.Add(new Solicitante { IdUsuario = usuario.IdUsuario, Nombre = nombre, Apellido = Limpio(datos.Apellido), Documento = documento, Telefono = telefono, Ciudad = ciudad });
        await db.SaveChangesAsync();

        return Ok(new { mensaje = "ok", response = new UsuarioResponse(usuario.IdUsuario, tipo, nombre, correo, telefono ?? "") });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest datos)
    {
        string correo = datos.Correo?.Trim().ToLowerInvariant() ?? "";
        var usuario = await db.Usuarios.AsNoTracking().Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == correo);

        if (usuario is null || !usuario.Activo || !Verificar(datos.Clave ?? "", usuario.PasswordHash))
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

        return Ok(new { mensaje = "ok", response = await RespuestaAsync(usuario) });
    }

    private async Task<UsuarioResponse> RespuestaAsync(Usuario u)
    {
        string tipo = u.Rol?.Nombre ?? "Usuario";
        (string? nombre, string? telefono) = tipo switch
        {
            "Asociacion" => await db.Asociaciones.Where(a => a.IdUsuario == u.IdUsuario).Select(a => new ValueTuple<string?, string?>(a.Nombre, a.Telefono)).FirstOrDefaultAsync(),
            "Veterinaria" => await db.Veterinarias.Where(v => v.IdUsuario == u.IdUsuario).Select(v => new ValueTuple<string?, string?>(v.Nombre, v.Telefono)).FirstOrDefaultAsync(),
            _ => await db.Solicitantes.Where(s => s.IdUsuario == u.IdUsuario).Select(s => new ValueTuple<string?, string?>(s.Nombre, s.Telefono)).FirstOrDefaultAsync(),
        };
        return new UsuarioResponse(u.IdUsuario, tipo, nombre ?? "", u.Email, telefono ?? "");
    }

    private static string? Limpio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static string Hashear(string clave)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(sal)}:{Convert.ToBase64String(hash)}";
    }

    private static bool Verificar(string clave, string guardado)
    {
        string[] partes = guardado.Split(':');
        if (partes.Length != 2)
            return false;

        byte[] sal = Convert.FromBase64String(partes[0]);
        byte[] esperado = Convert.FromBase64String(partes[1]);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, esperado);
    }
}
