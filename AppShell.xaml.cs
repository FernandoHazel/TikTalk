namespace TikTalk
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof (QReaderPage), typeof (QReaderPage));
            Routing.RegisterRoute(nameof(TimerPage), typeof(TimerPage));
        }
    }
}