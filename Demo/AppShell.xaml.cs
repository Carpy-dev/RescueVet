// Navegación: registra las pantallas que se abren encima de otra (detalles y formularios)
// y ofrece los atajos IrA y Abrir, este último para pasarle a una pantalla el elemento que debe mostrar.
namespace Demo;

public partial class AppShell : Shell
{
    // Rutas absolutas de cada pestaña (definidas en AppShell.xaml)
    public const string RutaLogin = "//login";
    public const string RutaInicio = "//app/inicio";
    public const string RutaAyuda = "//app/ayuda";
    public const string RutaDonacion = "//app/donacion";
    public const string RutaFavoritos = "//app/favoritos";
    public const string RutaPanel = "//panel/casos";

    // Clave con la que las pantallas de detalle reciben el elemento a mostrar
    public const string ParametroItem = "Item";

    public AppShell()
    {
        InitializeComponent();

        // Pantallas que se apilan encima de la actual (tienen botón "atrás")
        Registrar<RegistroPage>();

        Registrar<ConfiguracionPage>();

        Registrar<AsociacionesPage>();
        Registrar<AsociacionDetallePage>();

        Registrar<VeterinariasPage>();
        Registrar<VeterinariaDetallePage>();

        Registrar<CasoDetallePage>();
        Registrar<ReportarCasoPage>();

        Registrar<MiMascotaPage>();
        Registrar<HistorialMedicoPage>();
        Registrar<RecordatoriosPage>();
        Registrar<CrecimientoPage>();
    }

    // La ruta de cada pantalla es el nombre de su clase (ej. "CasoDetallePage")
    private static void Registrar<T>() where T : Page
        => Routing.RegisterRoute(typeof(T).Name, typeof(T));

    // Atajo para navegar desde cualquier pantalla
    public static Task IrA(string ruta) => Current.GoToAsync(ruta);

    // Abre una pantalla de detalle pasándole el elemento (la pantalla lo recibe en ApplyQueryAttributes)
    public static Task Abrir<T>(object item) where T : Page
        => Current.GoToAsync(typeof(T).Name, new Dictionary<string, object> { [ParametroItem] = item });
}
