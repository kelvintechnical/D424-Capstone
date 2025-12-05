using StudentProgressTracker.ViewModels;
using StudentProgressTracker.Services;

namespace StudentProgressTracker.Views;

public partial class GPAPage : ContentPage
{
	public GPAPage(GPAViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is GPAViewModel vm)
		{
			// Get current term ID from preferences (last viewed term)
			var termId = Preferences.Get("last_term_id", -1);
			if (termId > 0)
			{
				await vm.LoadGPADataAsync(termId);
			}
			else
			{
				// Try to get the first available term
				var db = ServiceHelper.GetRequiredService<DatabaseService>();
				var terms = await db.GetAllTermsAsync();
				if (terms.Count > 0)
				{
					await vm.LoadGPADataAsync(terms[0].Id);
				}
			}
		}
	}
}

