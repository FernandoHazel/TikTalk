using Microsoft.Extensions.Logging;
using TikTalk.ViewModel;
using Camera;
using Camera.MAUI;
using Plugin.LocalNotification;
using SQLite;

namespace TikTalk
{
    public static class MauiProgram
    {

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //Services
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<TimerPage>();
            
            //Create a path for the db
            string dbpath = FileAccessHelper.GetLocalFilePath("people.db3");

            //Instert the path in the main class
            builder.Services.AddSingleton<MainPageViewModel>
                (s => ActivatorUtilities.CreateInstance<MainPageViewModel>(s, dbpath));

            builder.Services.AddTransient<QReaderPage>
                (s => ActivatorUtilities.CreateInstance<QReaderPage>(s, dbpath));

            builder.Services.AddTransient
                <TimerPageViewModel>
            (s => ActivatorUtilities.CreateInstance<TimerPageViewModel>(s, dbpath));

            #if DEBUG
            builder.Logging.AddDebug();
            #endif
            return builder.Build();
        }
    }
}