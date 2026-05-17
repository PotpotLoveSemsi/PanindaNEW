using Paninda.Core.Views;
using Paninda.Services;

namespace Paninda;

public partial class App : Application
{
    public static AppDatabase Database { get; private set; } = null!;
    public static SupabaseService Supabase { get; private set; } = new();

    public App()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        Database = new AppDatabase(dbPath);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = new NavigationPage(new WelcomePage());
        navPage.BarBackgroundColor = Color.FromArgb("#FF69B4");
        navPage.BarTextColor = Colors.White;

        return new Window(navPage);
    }
}