using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace TikTalk
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            const int requestNotification = 0;
            const int requestExactAlarm = 0;
            string[] notiPermission =
            {
                Manifest.Permission.PostNotifications
            };

            if ((int)Build.VERSION.SdkInt < 33)
                return;

            if (CheckSelfPermission(Manifest.Permission.PostNotifications) == Permission.Granted)
                return;

            RequestPermissions(notiPermission, requestNotification);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("my_channel_id", "My Channel", NotificationImportance.Default)
                {
                    Description = "Channel description"
                };

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager.CreateNotificationChannel(channel);
            }

            // Request alarm permissions
            if (CheckSelfPermission(Manifest.Permission.ScheduleExactAlarm) != Permission.Granted)
            {
                // Permission is not granted
                // Request the permission
                RequestPermissions(
                        new String[] { Manifest.Permission.ScheduleExactAlarm }, requestExactAlarm);
            }

            //Handle alarm permissions
            //..


        }
    }
}