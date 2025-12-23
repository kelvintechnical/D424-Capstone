using StudentProgressTracker.Helpers;

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
		// Always start at login page - don't check authentication on start
		await MainThread.InvokeOnMainThreadAsync(async () =>
		{
			if (Shell.Current != null)
			{
				await Shell.Current.GoToAsync("//LoginPage");
			}
		});
	}

	protected override void OnSleep()
	{
		base.OnSleep();
		// Clear tokens on app close to force login on next launch
		_ = Task.Run(async () =>
		{
			try
			{
				var apiService = ServiceHelper.Services?.GetService<Services.ApiService>();
				if (apiService != null)
				{
					await apiService.LogoutAsync();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[App] Error clearing tokens on sleep: {ex.Message}");
			}
		});
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			var shell = new AppShell();
			var window = new Window(shell) { Title = "StudentProgressTracker" };
			
			// Make window visible and activate it
			window.Activated += async (s, e) =>
			{
#if DEBUG
				System.Diagnostics.Debug.WriteLine("[App] Window activated");
#endif
				// Always navigate to login page on window activation
				if (Shell.Current != null)
				{
					await Shell.Current.GoToAsync("//LoginPage");
				}
			};
			
			return window;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] CreateWindow failed: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			// Log to file for debugging
			try
			{
				var logPath = Path.Combine(FileSystem.AppDataDirectory, "error.log");
				File.WriteAllText(logPath, $"{DateTime.Now}: CreateWindow failed: {ex.Message}\n{ex.StackTrace}");
			}
			catch { }
			throw;
		}
	}
}

