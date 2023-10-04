using Camera.MAUI;
using TikTalk.ViewModel;

namespace TikTalk
{
    public partial class MainPage : ContentPage
    {
        CameraView cam;
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            // Store the camera view to pass it
            cam = cameraView;

        }

        private void OnCamerasLoaded(object sender, EventArgs e)
        {

            if (BindingContext is MainPageViewModel viewModel)
            {
                // Maneja el evento en el ViewModel
                viewModel.CamerasLoadedEventHandler(cam);
            }
            
        }
    }
}