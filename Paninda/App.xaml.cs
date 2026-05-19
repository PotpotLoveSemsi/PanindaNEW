using Paninda.Core.Views;
using Paninda.Services;

namespace Paninda;

public partial class App : Application
{
    public static AppDatabase Database { get; private set; } = null!;
    public static SupabaseService Supabase { get; private set; } = new();
    public static SupabaseProductService Products { get; private set; } = new();
    public static SupabaseOrderService Orders { get; private set; } = new();

    public static SaleDatabase Sales { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
        Database = new AppDatabase(dbPath);

        string salesDbPath = Path.Combine(FileSystem.AppDataDirectory, "sales.db3");
        Sales = new SaleDatabase(salesDbPath);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = new NavigationPage(new WelcomePage());
        navPage.BarBackgroundColor = Color.FromArgb("#FF69B4");
        navPage.BarTextColor = Colors.White;

        return new Window(navPage);
    }
}