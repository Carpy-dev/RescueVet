// Base de lo que se puede guardar en Favoritos (asociaciones, veterinarias y casos).
// FotoUrl convierte la ruta guardada en la base en la dirección que usa la imagen; EsFavorito actualiza el corazón al instante.
using System.ComponentModel;

namespace Demo.Models;

// Etiqueta de color ("Rescate", "Vacunación", ...)
public record Etiqueta(string Texto, Color ColorTexto, Color ColorFondo);

// Base de lo que puede aparecer en Favoritos (asociaciones, veterinarias, casos)
public abstract class ItemFavoritable : INotifyPropertyChanged
{
    private bool esFavorito;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Foto { get; set; } = "";
    public string FotoUrl => Api.UrlFoto(Foto);

    // Avisa a la pantalla para que el corazón cambie al instante
    public bool EsFavorito
    {
        get => esFavorito;
        set
        {
            if (esFavorito == value) return;
            esFavorito = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EsFavorito)));
        }
    }

    // Texto corto que se muestra en la lista de Favoritos
    public abstract string Resumen { get; }
}
