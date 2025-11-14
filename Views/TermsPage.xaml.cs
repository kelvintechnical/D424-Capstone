using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

public partial class TermsPage : ContentPage
{
	public TermsPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<TermsViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		var vm = (TermsViewModel)BindingContext;
		await vm.LoadTermsAsync();
		vm.LoadLastViewedTerm();
	}
}







