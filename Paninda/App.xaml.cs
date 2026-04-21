using Microsoft.Extensions.DependencyInjection;
using Paninda.Core.Views;

namespace Paninda
{
    public partial class App : Application
    {
        public static AppDatabase Database { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            Database = new AppDatabase(dbPath);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var navPage = new NavigationPage(new WelcomePage());
            navPage.BarBackgroundColor = Color.FromArgb("#FF69B4"); // pink
            navPage.BarTextColor = Colors.White;
            return new Window(navPage);
        }
    }
}