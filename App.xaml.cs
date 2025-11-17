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

	protected override Window CreateWindow(IActivationState? activationState)
	{
		try
		{
			var shell = new AppShell();
			return new Window(shell) { Title = "StudentProgressTracker" };
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[App] CreateWindow failed: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			throw;
		}
	}
}

