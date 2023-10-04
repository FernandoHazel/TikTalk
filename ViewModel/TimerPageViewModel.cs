using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.LocalNotification;
using System.Diagnostics;
using System.Reflection;
using SQLite;
using TikTalk.Models;
using System.IO;

namespace TikTalk.ViewModel
{
    //[QueryProperty("Timeleft", "Timeleft")]
    public partial class TimerPageViewModel:ObservableObject
    {
        string _dbpath;

        private SQLiteAsyncConnection connection;

        private string localImagePath;

        [ObservableProperty]
        string personName;

        [ObservableProperty]
        ImageSource personImage;

        [ObservableProperty]
        string timeleft;

        public static string time;

        [ObservableProperty]
        bool timeOver = false;

        [ObservableProperty]
        bool isCountingDown = false;

        public static System.Timers.Timer Timer;

        private TimeSpan countdownTime;
        private DateTime startTime;

        private int hours;
        private int minutes;
        private int seconds;

        public TimerPageViewModel(string dbpath)
        {
            _dbpath = dbpath;
            Timeleft = time;
            
            //Take the sting time and make it time variables
            FormatTime();
            RunTimer();

            //Update the image and name
            CheckMedia();

        }

        public async void CheckMedia()
        {
            await GetMedia();
        }

        private async Task CheckSQLiteAsyncConnection()
        {
            if (connection != null)
                return;

            connection = new SQLiteAsyncConnection(_dbpath);
            await connection.CreateTableAsync<Person>();
        }

        public async Task GetMedia()
        {
            try
            {
                await CheckSQLiteAsyncConnection();

                //Get data
                List<Person> persons = await connection.Table<Person>().ToListAsync();

                if (persons.Any())
                {
                    Person person = persons[0];

                    //If previous data was registered
                    //Fill the UI with that info
                    localImagePath = person.ImagePath;
                    PersonName = person.Name;

                    //Set the image on the UI
                    PersonImage = ImageSource.FromFile(localImagePath);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Something was wrong while getting the data");
            }
        }

        public void FormatTime() 
        {
            //Pase the string
            string[] timeParts = Timeleft.Split(':');

            if (timeParts.Length == 3)
            {
                if (int.TryParse(timeParts[0], out int hours) &&
                    int.TryParse(timeParts[1], out int minutes) &&
                    int.TryParse(timeParts[2], out int seconds))
                {
                    // Ahora tienes las horas en la variable "hours",
                    // los minutos en la variable "minutes" y
                    // los segundos en la variable "seconds".
                    // Configura el tiempo inicial del cronómetro (por ejemplo, 1 hora, 30 minutos y 0 segundos).
                    countdownTime = new TimeSpan(hours, minutes, seconds);

                    System.Diagnostics.Debug.WriteLine($"Horas: {hours}, Minutos: {minutes}, Segundos: {seconds}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("El formato del tiempo es incorrecto.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("El formato del tiempo no es válido.");
            }

            RunTimer();
        }

        public async void RunTimer()
        {
            TimeOver = false;

            //This happens every time the timer elapses
            if (!IsCountingDown)
            {
                // Registra la hora de inicio.
                startTime = DateTime.Now;

                // Inicia el cronómetro.
                IsCountingDown = true;

                // Actualiza el Label del cronómetro en un bucle hasta que llegue a cero.
                while (IsCountingDown)
                {

                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    TimeSpan remainingTime = countdownTime - elapsedTime;

                    if (remainingTime <= TimeSpan.Zero)
                    {
                        remainingTime = TimeSpan.Zero;
                        IsCountingDown = false;
                        Timeleft = "TIME IS OVER!";
                        TimeOver = true;
                        //Device.BeginInvokeOnMainThread(() => PlayAlarm());
                        await App.Current.MainPage.DisplayAlert("TIME IS OVER!", "Scan another QR code", "OK");
                        return;
                    }

                    Timeleft = remainingTime.ToString(@"hh\:mm\:ss");
                    await Task.Delay(1000); // Espera 1 segundo antes de actualizar de nuevo.
                }
            }
        }

        [RelayCommand]
        async Task ScanAgain()
        {
            await Shell.Current.GoToAsync($"{nameof(QReaderPage)}");
        }
    }
}
