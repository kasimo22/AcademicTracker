using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace AcademicTracker
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; private set; }

        static App()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "academic_tracker.db");
            System.Diagnostics.Debug.WriteLine($"Database Path: {dbPath}");
            Database = new DatabaseService(dbPath);
        }

        public App()
        {
            InitializeComponent();
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
            SeedDatabaseAsync();
        }

        private async void SeedDatabaseAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Seeding database...");
                await Database.SeedDatabaseAsync();

                // Notify the main page to refresh after seeding is complete
                if (MainPage is AppShell shell)
                {
                    if (shell.CurrentPage is MainPage mainPage)
                    {
                        mainPage.RefreshTerms();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database seeding failed: {ex.Message}");
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void OnNotificationTapped(NotificationActionEventArgs e)
        {
            if (e.Request.ReturningData is string data)
            {
                Shell.Current.GoToAsync(data); // Navigate based on data, if provided
            }
        }
    }
}