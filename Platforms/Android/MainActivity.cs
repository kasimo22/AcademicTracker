using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace AcademicTracker
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create the notification channel only if the Android version supports it
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateDefaultNotificationChannel();
            }
        }

        private void CreateDefaultNotificationChannel()
        {
            const string channelId = "default";
            const string channelName = "Default Notifications";
            const string channelDescription = "General notifications for the app";

            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
            {
                Description = channelDescription
            };

            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);

            System.Diagnostics.Debug.WriteLine("Notification channel created.");
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            // Handle notification tap actions
            if (intent?.Extras != null && intent.Extras.ContainsKey("ReturningData"))
            {
                var data = intent.Extras.GetString("ReturningData");

                if (!string.IsNullOrEmpty(data))
                {
                    // Navigate to the page specified in the notification data
                    Shell.Current.GoToAsync(data);
                }
            }
        }
    }
}

