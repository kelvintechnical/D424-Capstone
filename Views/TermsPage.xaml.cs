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
		try
		{
			if (BindingContext is TermsViewModel vm)
			{
				await vm.LoadTermsAsync();
				vm.LoadLastViewedTerm();
			}
		}
		catch (Exception ex)
		{
			// Handle database initialization errors gracefully
			System.Diagnostics.Debug.WriteLine($"[TermsPage] Error loading terms: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[TermsPage] Stack trace: {ex.StackTrace}");
			// The page will still display, just with an empty list
			// User can try adding a new term which will trigger database initialization
		}
	}
}







