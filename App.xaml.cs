namespace StudentProgressTracker;

public partial class App : Application
{
	public App()
	{
		try
		{
			InitializeComponent();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] InitializeComponent failed: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			throw;
		}
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await CheckAuthenticationAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			var shell = new AppShell();
			var window = new Window(shell) { Title = "StudentProgressTracker" };
			
			// Check authentication on window creation
			_ = Task.Run(async () => await CheckAuthenticationAsync());
			
			return window;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] CreateWindow failed: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			throw;
		}
	}

	private async Task CheckAuthenticationAsync()
	{
		try
		{
			// Wait a bit for services to be ready
			await Task.Delay(100);
			
			var apiService = ServiceHelper.Services?.GetService<Services.ApiService>();
			if (apiService == null)
			{
				// Services not ready yet, navigate to login
				await MainThread.InvokeOnMainThreadAsync(async () =>
				{
					if (Shell.Current != null)
					{
						await Shell.Current.GoToAsync("//LoginPage");
					}
				});
				return;
			}

			var isAuthenticated = await apiService.IsAuthenticatedAsync();
			
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				if (Shell.Current != null)
				{
					if (isAuthenticated)
					{
						// User is authenticated, go to main app
						await Shell.Current.GoToAsync("//TermsPage");
					}
					else
					{
						// User not authenticated, go to login
						await Shell.Current.GoToAsync("//LoginPage");
					}
				}
			});
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] CheckAuthenticationAsync failed: {ex.Message}");
			// On error, default to login page
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				if (Shell.Current != null)
				{
					await Shell.Current.GoToAsync("//LoginPage");
				}
			});
		}
	}
}

