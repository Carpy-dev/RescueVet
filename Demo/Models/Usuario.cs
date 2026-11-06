// Datos de la cuenta que inició sesión, devueltos por la API. No incluye la contraseña.
namespace Demo.Models;

public record Usuario(int Id, string Tipo, string Nombre, string Correo, string Telefono);
