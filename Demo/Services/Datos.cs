// Datos en memoria que comparten las pantallas. CargarAsync los trae del backend al iniciar sesión;
// también maneja los cambios de Mi Mascota y agrega o quita favoritos, guardándolos en la base.
using System.Collections.ObjectModel;

namespace Demo.Services;

// Datos de la app: se cargan desde el backend al iniciar sesión.
public static class Datos
{
    public static ObservableCollection<Asociacion> Asociaciones { get; } = [];
    public static ObservableCollection<Veterinaria> Veterinarias { get; } = [];
    public static ObservableCollection<Caso> Casos { get; } = [];
    public static ObservableCollection<ItemFavoritable> Favoritos { get; } = [];
    public static ObservableCollection<Campana> Campanas { get; } = [];
    public static ObservableCollection<Novedad> Novedades { get; } = [];
    public static Mascota? MiMascota { get; private set; }

    // Lanza ApiException si no se puede conectar
    public static async Task CargarAsync()
    {
        var asociaciones = Api.Lista<Asociacion>();
        var veterinarias = Api.Lista<Veterinaria>();
        var casos = Api.EsAsociacion ? Api.Lista<Caso>() : Task.FromResult(new List<Caso>());
        var campanas = Api.Lista<Campana>();
        var novedades = Api.Lista<Novedad>();
        var mascotas = MisMascotasAsync();
        await Task.WhenAll(asociaciones, veterinarias, casos, campanas, novedades, mascotas);

        Reemplazar(Asociaciones, asociaciones.Result);
        Reemplazar(Veterinarias, veterinarias.Result);
        Reemplazar(Casos, casos.Result);
        Reemplazar(Campanas, campanas.Result);
        Reemplazar(Novedades, novedades.Result);
        MiMascota = mascotas.Result.FirstOrDefault();

        var favoritos = Asociaciones.Concat<ItemFavoritable>(Veterinarias).Concat(Casos).Where(i => i.EsFavorito);
        Reemplazar(Favoritos, favoritos.ToList());
    }

    // Ejecuta un cambio de Mi Mascota y vuelve a leerla de la base. Devuelve false si falló.
    public static async Task<bool> CambiarMascota(Func<Task> cambio)
    {
        try
        {
            await cambio();
            MiMascota = (await MisMascotasAsync()).FirstOrDefault();
            return true;
        }
        catch (ApiException e)
        {
            await Shell.Current.DisplayAlertAsync("Mi Mascota", e.Message, "OK");
            return false;
        }
    }

    // Cada mascota pertenece a una cuenta. Sin cuenta, no hay mascotas para mostrar.
    private static Task<List<Mascota>> MisMascotasAsync()
        => Api.UsuarioActual is null ? Task.FromResult(new List<Mascota>()) : Api.Lista<Mascota>();

    private static void Reemplazar<T>(ObservableCollection<T> destino, IEnumerable<T> nuevos)
    {
        destino.Clear();
        foreach (var item in nuevos)
            destino.Add(item);
    }

    // Agrega o quita un elemento de Favoritos y lo guarda en el backend
    public static async Task AlternarFavorito(ItemFavoritable item)
    {
        bool valor = !item.EsFavorito;
        try
        {
            await Api.MarcarFavorito(item, valor);
        }
        catch (ApiException e)
        {
            await Shell.Current.DisplayAlertAsync("Favoritos", e.Message, "OK");
            return;
        }

        item.EsFavorito = valor;
        if (valor)
            Favoritos.Add(item);
        else
            Favoritos.Remove(item);
    }
}
