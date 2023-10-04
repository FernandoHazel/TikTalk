using TikTalk.ViewModel;

namespace TikTalk
{
    public partial class App : Application
    {
        public static MainPageViewModel PersonRepo { get; private set; }
            
        public App(MainPageViewModel repo)
        {
            InitializeComponent();

            MainPage = new AppShell();

            PersonRepo = repo;
        }
        
    }
}