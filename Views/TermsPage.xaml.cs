using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

public partial class TermsPage : ContentPage
{
	public TermsPage()
	{
		try
		{
			InitializeComponent();
			
			// Try to get ViewModel with error handling
			try
			{
				if (ServiceHelper.Services != null)
				{
					BindingContext = ServiceHelper.GetRequiredService<TermsViewModel>();
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("[TermsPage] ServiceHelper.Services is null");
					// Create a minimal ViewModel instance as fallback
					var db = new Services.DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "student-progress.db3"));
					BindingContext = new TermsViewModel(db);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[TermsPage] Failed to get ViewModel: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"[TermsPage] Stack trace: {ex.StackTrace}");
				// Create a minimal ViewModel instance as fallback
				try
				{
					var db = new Services.DatabaseService(Path.Combine(FileSystem.AppDataDirectory, "student-progress.db3"));
					BindingContext = new TermsViewModel(db);
				}
				catch (Exception ex2)
				{
					System.Diagnostics.Debug.WriteLine($"[TermsPage] Failed to create fallback ViewModel: {ex2.Message}");
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[TermsPage] Constructor error: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[TermsPage] Stack trace: {ex.StackTrace}");
			// Re-throw to see the full error in logs
			throw;
		}
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







