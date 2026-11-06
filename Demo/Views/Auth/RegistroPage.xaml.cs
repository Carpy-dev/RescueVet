// Lógica del registro: controla los pasos, adapta los campos al tipo de cuenta y crea la cuenta en la API.
namespace Demo.Views.Auth;

// Registro en 3 pasos: 1) tipo de cuenta  2) datos  3) correo y contraseña  → pantalla final.
// Al terminar el paso 3 crea la cuenta en el backend.
public partial class RegistroPage : ContentPage
{
    private TipoCuenta tipo = TipoCuenta.Usuario;
    private int paso = 1;
    private bool terminado;

    public RegistroPage()
    {
        InitializeComponent();
        PintarTipos();
        MostrarPaso();
    }

    private void MostrarPaso()
    {
        Paso1.IsVisible = paso == 1 && !terminado;
        Paso2.IsVisible = paso == 2 && !terminado;
        Paso3.IsVisible = paso == 3 && !terminado;
        Exito.IsVisible = terminado;

        Color activo = (Color)Application.Current!.Resources["LoginAzul"];
        Color inactivo = Color.FromArgb("#A9D9F2");
        Barra1.Color = activo;
        Barra2.Color = paso >= 2 || terminado ? activo : inactivo;
        Barra3.Color = paso >= 3 || terminado ? activo : inactivo;

        PasoTexto.Text = terminado ? "Listo" : $"Paso {paso} de 3";
        Atras.IsVisible = !terminado;
        LinkLogin.IsVisible = paso == 1 && !terminado;

        (TituloPaso.Text, SubtituloPaso.Text) = paso switch
        {
            1 => ("Creá tu cuenta", "Elegí el tipo de cuenta que mejor te describe."),
            2 => (tipo == TipoCuenta.Usuario ? "Tus datos" : "Datos de la organización",
                  tipo == TipoCuenta.Usuario
                      ? "Los usamos para que asociaciones y veterinarias puedan contactarte."
                      : "Así la comunidad puede encontrarte y contactarte."),
            _ => ("Datos de acceso", "Con estos datos vas a iniciar sesión."),
        };
        TituloPaso.IsVisible = SubtituloPaso.IsVisible = !terminado;

        BotonSiguiente.Text = terminado ? "Ir a iniciar sesión" : paso == 3 ? "Crear cuenta" : "Continuar";
    }

    private async void OnSiguienteClicked(object? sender, EventArgs e)
    {
        if (terminado)
        {
            await VolverAlLogin();
            return;
        }

        if (paso == 1)
            PrepararPaso2();

        if (paso == 3)
        {
            if (!await CrearCuenta())
                return;
            terminado = true;
        }
        else
            paso++;
        MostrarPaso();
    }

    private async Task<bool> CrearCuenta()
    {
        if (Contraseña.Texto != Confirmar.Texto)
        {
            await DisplayAlertAsync("Crear cuenta", "Las contraseñas no coinciden.", "OK");
            return false;
        }

        try
        {
            await Api.Registrar(new
            {
                tipo = tipo.ToString(),
                nombre = Nombre.Texto,
                apellido = Apellido.Texto,
                documento = Documento.Texto,
                matricula = Matricula.Texto,
                telefono = Telefono.Texto,
                ciudad = Ciudad.Texto,
                correo = Correo.Texto,
                clave = Contraseña.Texto,
            });
            return true;
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Crear cuenta", ex.Message, "OK");
            return false;
        }
    }

    private async void OnAtrasTocado(object? sender, EventArgs e)
    {
        if (paso > 1)
        {
            paso--;
            MostrarPaso();
        }
        else
        {
            await VolverAlLogin();
        }
    }

    private void OnTipoTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string nombre)
        {
            tipo = Enum.Parse<TipoCuenta>(nombre);
            PintarTipos();
        }
    }

    // La tarjeta elegida se marca con borde azul y un check
    private void PintarTipos()
    {
        Color azul = (Color)Application.Current!.Resources["LoginAzul"];
        Color borde = Color.FromArgb("#DCE6EE");

        foreach (var (card, check, t) in new[]
        {
            (CardUsuario, CheckUsuario, TipoCuenta.Usuario),
            (CardAsociacion, CheckAsociacion, TipoCuenta.Asociacion),
            (CardVeterinaria, CheckVeterinaria, TipoCuenta.Veterinaria),
        })
        {
            bool elegido = tipo == t;
            card.Stroke = elegido ? azul : borde;
            card.StrokeThickness = elegido ? 2.5 : 1.5;
            check.Text = elegido ? "" : ""; // check_circle / radio_button_unchecked
            check.TextColor = elegido ? azul : borde;
        }
    }

    // Las organizaciones ven nombre y CUIT; las veterinarias además la matrícula
    private void PrepararPaso2()
    {
        bool esPersona = tipo == TipoCuenta.Usuario;
        Apellido.IsVisible = esPersona;
        Matricula.IsVisible = tipo == TipoCuenta.Veterinaria;

        Nombre.Etiqueta = esPersona ? "Nombre" : tipo == TipoCuenta.Veterinaria ? "Nombre de la veterinaria" : "Nombre de la asociación";
        Nombre.Placeholder = esPersona ? "Tu nombre" : "Como aparece en tus redes";
        Nombre.Glifo = esPersona ? "" : "";
        Documento.Etiqueta = esPersona ? "DNI" : "CUIT";
        Documento.Placeholder = esPersona ? "Sin puntos" : "Sin guiones (11 números)";
    }

    private async void OnTerminosTapped(object? sender, TappedEventArgs e)
        => await DisplayAlertAsync("Términos y privacidad",
            "Usamos tus datos solo para conectarte con asociaciones, veterinarias y personas que necesitan ayuda. " +
            "No los compartimos con terceros. Podés pedir que borremos tu cuenta cuando quieras.", "Entendido");

    private async void OnIniciarSesionTapped(object? sender, TappedEventArgs e) => await VolverAlLogin();

    private static Task VolverAlLogin() => Shell.Current.GoToAsync("..");
}
