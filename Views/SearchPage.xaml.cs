using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

public partial class SearchPage : ContentPage
{
	public SearchPage(SearchViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}

