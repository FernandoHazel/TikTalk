using TikTalk.ViewModel;

namespace TikTalk;

public partial class TimerPage : ContentPage
{
	public TimerPage(TimerPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}