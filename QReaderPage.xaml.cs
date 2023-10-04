using Camera.MAUI;
using Plugin.LocalNotification;
using SQLite;
using TikTalk.Models;
using TikTalk.ViewModel;

namespace TikTalk;

public partial class QReaderPage : ContentPage
{
    string _dbpath;

    private SQLiteAsyncConnection connection;
    public QReaderPage(string dbpath)
	{
        _dbpath = dbpath;

		InitializeComponent();
    }



    private void cameraView_CamerasLoaded(object sender, EventArgs e)
    {
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }

    private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            //Take the text of the qr
            string qrResult = barcodeResult.Text;
            qrResult = $"{args.Result[0].Text}";

            //Check if the qr result has the desired format
            //Pase the string
            string[] timeParts = qrResult.Split(':');

            if (timeParts.Length == 3)
            {
                if (int.TryParse(timeParts[0], out int hours) &&
                    int.TryParse(timeParts[1], out int minutes) &&
                    int.TryParse(timeParts[2], out int seconds))
                {
                    //Asing a static variable to pass info to other clases
                    TimerPageViewModel.time = qrResult;

                    //Display the result in the UI
                    barcodeResult.Text = $"Horas: {hours}, Minutos: {minutes}, Segundos: {seconds}";

                    //Create the notification
                    await CreateNotification(new TimeSpan(hours, minutes, seconds));

                    //If everything is ok go to the timer page
                    await GoToTimer();
                }
                else
                {
                    barcodeResult.Text = $"Invalid QR";
                }
            }
            else
            {
                barcodeResult.Text = $"Invalid QR";
            }

            
        });
    }

    async Task CreateNotification(TimeSpan timeToAdd)
    {
        //Ask for permission
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        //Create the notification
        var request = new NotificationRequest
        {
            NotificationId = 100,
            Title = "Your time is over!",
            Description = "Open your app to scan another QR",
            ReturningData = "Dummy data", // Returning data when tapped on notification.
            BadgeNumber = 42,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.Add(timeToAdd),
            }
        };

        //Register notification in db
        await RegisterNotificationInDB(timeToAdd);

        //Program the notification
        await LocalNotificationCenter.Current.Show(request);
    }

    public async Task RegisterNotificationInDB(TimeSpan timeToAdd)
    {
        try
        {
            await CheckSQLiteAsyncConnection();

            //Get the person
            List<Person> persons = await connection.Table<Person>().ToListAsync();

            if (persons.Any())
            {
                //Update the notification time in the db
                Person person = persons[0];
                person.scheduledNotification = DateTime.Now.Add(timeToAdd);
                await connection.UpdateAsync(person);
            }

        }
        catch (Exception ex)
        {
            throw new Exception("Something was wrong while registering the notification");
        }
    }

    private async Task CheckSQLiteAsyncConnection()
    {
        if (connection != null)
            return;

        connection = new SQLiteAsyncConnection(_dbpath);
        await connection.CreateTableAsync<Person>();
    }

    private async Task GoToTimer()
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(TimerPage)}");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("An error ocurrer while going to timer view", $"{ex}", "ok");
        }
       
    }
}