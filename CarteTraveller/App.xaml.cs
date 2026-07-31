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
            // On enregistre le contexte d'abord
            services.AddSingleton<ICampaignContext, CampaignContext>();

            // On enregistre le manager ensuite
            services.AddSingleton<IGalaxyProvider, GalaxyManager>();

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
