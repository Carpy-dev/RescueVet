// Punto de entrada de la API: registra los servicios y la conexión a SQL Server,
// crea o completa la base al arrancar (ver Semilla.cs), activa Swagger y publica los controladores.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueVet.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Los campos de texto vacíos se aceptan; lo obligatorio se valida en cada controlador
builder.Services.AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL")));

var app = builder.Build();

// Crea la base RescueVet y sus datos iniciales la primera vez que arranca
using (var scope = app.Services.CreateScope())
    Semilla.Cargar(scope.ServiceProvider.GetRequiredService<AppDbContext>(), app.Configuration.GetValue<bool>("SinSemilla"));

// Para la migración: solo crea las tablas y termina
if (app.Configuration.GetValue<bool>("SinSemilla"))
    return;

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
