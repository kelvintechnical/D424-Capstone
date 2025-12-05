namespace StudentProgressTracker;

public partial class AppShell : Shell
{
	public AppShell()
	{
		try
		{
			InitializeComponent();
			RegisterRoutes();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[AppShell] Constructor failed: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
			throw;
		}
	}

	private void RegisterRoutes()
	{
		try
		{
			// Auth routes
			Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
			Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
			
			// Main app routes
			Routing.RegisterRoute(nameof(Views.CourseListPage), typeof(Views.CourseListPage));
			Routing.RegisterRoute(nameof(Views.TermDetailPage), typeof(Views.TermDetailPage));
			Routing.RegisterRoute(nameof(Views.CourseDetailPage), typeof(Views.CourseDetailPage));
			Routing.RegisterRoute(nameof(Views.AssessmentsPage), typeof(Views.AssessmentsPage));
			Routing.RegisterRoute(nameof(Views.GPAPage), typeof(Views.GPAPage));
			Routing.RegisterRoute(nameof(Views.SearchPage), typeof(Views.SearchPage));
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[AppShell] RegisterRoutes failed: {ex.Message}");
		}
	}
}


