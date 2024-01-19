using Camera.MAUI;
using Plugin.LocalNotification;
using SQLite;
using TikTalk.Models;
using TikTalk.ViewModel;

namespace TikTalk;

public partial class QReaderPage : ContentPage
{
    string _dbpath;
    public static int hrs;
    public static int min;
    public static int sec;
    public static bool isTimerActive = false;

    Responsive res = new Responsive();

    private SQLiteAsyncConnection connection;
    public QReaderPage(string dbpath)
	{
        _dbpath = dbpath;
        
		InitializeComponent();

        AdjustView();
    }

    private void AdjustView()
    {
        mainGrid.RowDefinitions = res.qrRows();
        mainGrid.ColumnDefinitions = res.qrColums();
        mainStackLayout.Spacing = res.qrSpacing();
    }

    private async void cameraView_CamerasLoaded(object sender, EventArgs e)
    {
        startButton.IsVisible = false;
        startButton.IsEnabled = false;

        try
        {
            if (cameraView.Cameras.Count > 0)
            {
                cameraView.Camera = cameraView.Cameras.First();

                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();

            }
        } catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert
                ("Ocurrió un error al iniciar la cámara: ", $"{ex}", "ok");
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
                    barcodeResult.Text = $"Hrs: {hours}, Min: {minutes}, Sec: {seconds}";

                    hrs = hours;
                    min = minutes;
                    sec = seconds;

                    startButton.IsVisible = true;
                    startButton.IsEnabled = true;
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

    public static void ResetTime()
    {
        hrs = 0;
        min = 0;
        sec = 0;
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
            BadgeNumber = 1,
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
            throw new Exception($"Something was wrong while registering the notification: {ex}");
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
            await Shell.Current.GoToAsync($"{nameof(TimerPage)}"); ///<---- Aquí fué el último brake point antes de cerrar
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("An error ocurrer while going to timer view", $"{ex}", "ok");
        }
       
    }

    private async void SetExpirationDate(TimeSpan timeToAdd)
    {
        DateTime expirationDate = DateTime.Now.Add(timeToAdd);

        try
        {
            await CheckSQLiteAsyncConnection();

            List<Person> persons = await connection.Table<Person>().ToListAsync();

            //If person exists update data
            if (persons.Any())
            {
                persons[0].ExpirationDate = expirationDate;

                await connection.UpdateAsync(persons[0]);
            }
            
        }
        catch (Exception ex)
        {
            throw new Exception($"Something was wrong while updating the data: {ex}");
        }
    }

    private async void startButton_Clicked(object sender, EventArgs e)
    {
        //Create the notification
        //await CreateNotification(new TimeSpan(hrs, min, sec)); //<-- La notificación está dando un problema de permisos

        //If everything is ok go to the timer page
        if (!isTimerActive) {

            // Set expiration date
            SetExpirationDate(new TimeSpan(hrs, min, sec));

            await GoToTimer();
        }
        else
        {
            await App.Current.MainPage.DisplayAlert("Hay otro temporalizador corriendo", $"{TimerPageViewModel.time}", "ok");
        }
    }
}