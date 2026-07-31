using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using CarteTraveller.Services;
using CarteTraveller.Models;

namespace CarteTraveller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Enregistrement des services (Ton infrastructure et domaine)
            // AddSingleton signifie qu'on aura une seule instance partagée dans toute l'app
            services.AddSingleton<IGalaxyProvider>(provider =>
                new GalaxyManager(@"C:\Traveller\Saves\MaCampagne")
            );

            // On peut aussi enregistrer des services sans interface si nécessaire
            services.AddTransient<SectorGeneratorService>();

            // On enregistre la fenêtre elle-même pour qu'elle profite de l'injection
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 3. On demande au conteneur de nous fournir la fenêtre principale, 
            // qui recevra automatiquement ses dépendances.
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

    }

}
