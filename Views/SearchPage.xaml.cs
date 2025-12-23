using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

public partial class SearchPage : ContentPage
{
	public SearchPage(SearchViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}

