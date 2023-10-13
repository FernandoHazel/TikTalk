using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using TikTalk.Models;
using Camera.MAUI;
using ImageFormat = Camera.MAUI.ImageFormat;

namespace TikTalk.ViewModel
{
    public partial class MainPageViewModel: ObservableObject
    {
        string _dbpath;

        ImageSource profileImage = ImageSource.FromFile("touchphoto.png");

        [ObservableProperty]
        ImageSource personImage = ImageSource.FromFile("touchphoto.png");

        [ObservableProperty]
        string personName;

        [ObservableProperty]
        bool formFilled = false;

        [ObservableProperty]
        bool camera = false;

        [ObservableProperty]
        bool image = false;

        [ObservableProperty]
        string localImagePath;

        [ObservableProperty]
        CameraView cameraView;

        //These are the responsive variables
        [ObservableProperty]
        RowDefinitionCollection rows;
        [ObservableProperty]
        ColumnDefinitionCollection columns;
        [ObservableProperty]
        double spacing;

        private SQLiteAsyncConnection connection;

        private Responsive res = new Responsive();

        public MainPageViewModel(string dbPath)
        {
            PropertyChanged += OnPropertyChanged;

            _dbpath = dbPath;

            AdjustView();
            CheckPerson();
        }

        private void AdjustView()
        {
            //This makes the view responsive
            Rows = res.rows();
            Columns = res.colums();
            Spacing = res.spacing();
        }

        //We add this intermediate method because cannot run a Task in the view model constructor
        public async void CheckPerson()
        {
            await GetPerson();
        }

        private async Task CheckSQLiteAsyncConnection()
        {
            if (connection != null)
                return;

            connection = new SQLiteAsyncConnection(_dbpath);
            await connection.CreateTableAsync<Person>();
        }

        public async Task GetPerson()
        {
            try
            {
                await CheckSQLiteAsyncConnection();

                //Get data
                List<Person> persons = await connection.Table<Person>().ToListAsync();

                if (persons.Any())
                {
                    Image = true;
                    Camera = false;

                    Person person = persons[0];

                    //If previous data was registered
                    //Fill the UI with that info
                    LocalImagePath = person.ImagePath;
                    PersonName = person.Name;

                    //Set the image on the UI
                    PersonImage = ImageSource.FromFile(LocalImagePath);
                    

                    //If there is a notification scheduled go to the timer view
                    if (person.scheduledNotification > DateTime.Now)
                    {
                        //Calculate time remaining
                        TimeSpan remainingTime = person.scheduledNotification - DateTime.Now;
                        string timeLeft = remainingTime.ToString(@"hh\:mm\:ss");

                        //Go to the timer view
                        try
                        {
                            //await Shell.Current.GoToAsync($"{nameof(TimerPage)}?Timeleft={timeLeft}");
                            TimerPageViewModel.time = timeLeft;
                            await Shell.Current.GoToAsync($"{nameof(TimerPage)}");
                        }
                        catch (Exception ex)
                        {
                            await App.Current.MainPage.DisplayAlert("An error ocurrer while going to timer view", $"{ex}", "ok");
                        }
                        
                    }

                }
                else
                {
                    Image = false;
                    Camera = true;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Something was wrong while getting the data");
            }
        }

        public async Task UpdatePerson(string name, string imagePath)
        {
            int result = 0;
            try
            {
                await CheckSQLiteAsyncConnection();

                //Ensure name was entered
                if (string.IsNullOrEmpty(name))
                    throw new Exception("Valid name required");

                //Ensure an image path was entered
                if (string.IsNullOrEmpty(imagePath))
                    throw new Exception("Valid image");

                //If the person is new insert a new one
                //If already exist only update it
                List<Person> persons = await connection.Table<Person>().ToListAsync();

                //If person exists update data
                if (persons.Any())
                {
                    persons[0].ImagePath = imagePath;
                    persons[0].Name = name;
                    persons[0].scheduledNotification = DateTime.Now;

                    await connection.UpdateAsync(persons[0]);

                    //await App.Current.MainPage.DisplayAlert("Updated", $"{persons[0]}", "ok");
                }
                else
                {
                    //Make sure there are not residual fields
                    //From previous interactions
                    await connection.DeleteAllAsync<Person>();

                    //If this is the first time insert new data
                    result = await connection.InsertAsync(new Person
                    {
                        Name = name,
                        ImagePath = imagePath,
                        scheduledNotification = DateTime.Now,
                    });

                    //await App.Current.MainPage.DisplayAlert("Inserted", $"{result} person", "ok");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Something was wrong while updating the data");
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PersonName) || e.PropertyName == nameof(PersonImage))
            {
                //Verify if the form is complete
                VerifyForm();
            }
        }

        //Not in use at the moment 
        public void CamerasLoadedEventHandler(CameraView cam)
        {
            CameraView = cam;
            
            // Select the fist camera once it is loaded
            if(CameraView != null)
            {
                CameraView.MirroredImage = false;
                if (CameraView.Cameras.Count > 1)
                {
                    // Selecciona la cámara frontal
                    CameraView.Camera = CameraView.Cameras[1]; // Índice 1 generalmente corresponde a la cámara frontal
                }
                else
                {
                    // Si solo hay una cámara disponible, selecciona esa
                    CameraView.Camera = CameraView.Cameras[0];
                }
            }
                

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (await CameraView.StopCameraAsync() == CameraResult.Success)
                {
                    Camera = false;
                }
                if (await CameraView.StartCameraAsync() == CameraResult.Success)
                {
                    Camera = true;
                }
                
            });
        }

        
        [RelayCommand]
        async Task TakePhoto()
        {
            
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var stream = await CameraView.TakePhotoAsync();

                if (stream != null)
                {
                    // Define the name and path of the new image
                    string imageName = $"image_{DateTime.Now.Ticks}.png"; // Agrega un timestamp único al nombre de la imagen
                    LocalImagePath = Path.Combine(FileSystem.CacheDirectory, imageName);

                    // Store the new one
                    using FileStream localFileStream = File.OpenWrite(LocalImagePath);
                    await stream.CopyToAsync(localFileStream);

                    // Set the new file to the view on the UI
                    PersonImage = ImageSource.FromFile(LocalImagePath);

                    //Disabe the camera and enable the photo view
                    Image = true;
                    Camera = false;
                }

            }

        }

        [RelayCommand]
        void RetakePhoto()
        {

            Camera = true;
            Image = false;

        }


        private void VerifyForm()
        {
            //If the person already added his photo and name
            if (AreImageSourcesDifferent(PersonImage, profileImage) && !string.IsNullOrEmpty(PersonName))
            {
                FormFilled = true;
            }
            else
            {
                FormFilled = false;
            }
        }

        private bool AreImageSourcesDifferent(ImageSource imageSource1, ImageSource imageSource2)
        {
            if (imageSource1 is FileImageSource fileImageSource1 &&
                imageSource2 is FileImageSource fileImageSource2)
            {
                return fileImageSource1.File != fileImageSource2.File;
            }

            // If the types of ImageSource are different, consider them different
            return true;
        }

        [RelayCommand]
        async Task GoToScanQR()
        {
            //Add the person to the data base
            await UpdatePerson(PersonName, LocalImagePath);

            //Go to the next view
            await Shell.Current.GoToAsync($"{nameof(QReaderPage)}");
        }

    }

}