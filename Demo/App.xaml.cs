// Inicio de la aplicación: crea la ventana principal y carga AppShell, que maneja la navegación.
using Microsoft.Extensions.DependencyInjection;

namespace Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}