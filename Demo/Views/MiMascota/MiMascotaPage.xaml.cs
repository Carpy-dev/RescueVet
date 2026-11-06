// Lógica de Mi Mascota: muestra los datos actuales, edita el perfil y abre las sub pantallas.
namespace Demo.Views.MiMascota;

public partial class MiMascotaPage : ContentPage
{
    public MiMascotaPage()
    {
        InitializeComponent();
    }

    // Al volver de una sub pantalla muestra los datos actualizados
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Mostrar();
    }

    private void Mostrar()
    {
        bool hayMascota = Datos.MiMascota is not null;
        bool hayCuenta = Api.UsuarioActual is not null;
        ContenidoMascota.IsVisible = hayMascota;
        SinMascota.IsVisible = !hayMascota;
        BotonAgregar.IsVisible = hayCuenta;
        MensajeSinMascota.Text = hayCuenta ? "Todavía no cargaste ninguna mascota." : "Iniciá sesión con tu cuenta para ver tus mascotas.";

        BindingContext = null;
        BindingContext = Datos.MiMascota;
    }

    private async void OnAgregarClicked(object? sender, EventArgs e)
    {
        if (Api.UsuarioActual is not { } usuario)
            return;

        var nombre = new CampoForm("Nombre");
        var raza = new CampoForm("Raza");
        var edad = new CampoForm("Edad");
        var sexo = new CampoForm("Sexo");
        if (!await FormularioPage.Pedir("Agregar mascota", nombre, raza, edad, sexo))
            return;

        var datos = new { usuarioId = usuario.Id, nombre = nombre.Valor, raza = raza.Valor, edad = edad.Valor, sexo = sexo.Valor };
        if (await Datos.CambiarMascota(() => Api.Guardar("Mascota", datos)))
            Mostrar();
    }

    private async void OnEditarTapped(object? sender, TappedEventArgs e)
    {
        if (Datos.MiMascota is not { } m)
            return;

        var nombre = new CampoForm("Nombre", m.Nombre);
        var raza = new CampoForm("Raza", m.Raza);
        var edad = new CampoForm("Edad", m.Edad);
        var sexo = new CampoForm("Sexo", m.Sexo);
        var rango = new CampoForm("Rango de peso ideal", m.RangoPeso);
        var estadoPeso = new CampoForm("Estado del peso", m.EstadoPeso);
        if (!await FormularioPage.Pedir("Editar mascota", nombre, raza, edad, sexo, rango, estadoPeso))
            return;

        // Vacunas y alergias son parte del historial médico: se envían sin cambios
        var datos = new
        {
            m.Foto, m.Consejos, m.Galeria, m.EstadoVacunas, m.Alergias,
            Nombre = nombre.Valor, Raza = raza.Valor, Edad = edad.Valor, Sexo = sexo.Valor,
            RangoPeso = rango.Valor, EstadoPeso = estadoPeso.Valor,
        };
        if (await Datos.CambiarMascota(() => Api.Editar("Mascota", m.Id, datos)))
            Mostrar();
    }

    // Cada mosaico abre su sub pantalla: el CommandParameter es el nombre de la ruta (ej. "CrecimientoPage")
    private async void OnAccesoTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string ruta)
            await Shell.Current.GoToAsync(ruta);
    }

    private async void OnConsejosClicked(object? sender, EventArgs e)
    {
        if (Datos.MiMascota is { } mascota)
            await DisplayAlertAsync($"Consejos para {mascota.Nombre}",
                mascota.Consejos.Length == 0 ? "Todavía no hay consejos cargados." : string.Join("\n", mascota.Consejos.Select(c => $"• {c}")), "OK");
    }
}
