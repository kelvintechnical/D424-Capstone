namespace StudentProgressTracker;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		RegisterRoutes();
	}

	private void RegisterRoutes()
	{
		Routing.RegisterRoute(nameof(Views.CourseListPage), typeof(Views.CourseListPage));
		Routing.RegisterRoute(nameof(Views.TermDetailPage), typeof(Views.TermDetailPage));
		Routing.RegisterRoute(nameof(Views.CourseDetailPage), typeof(Views.CourseDetailPage));
		Routing.RegisterRoute(nameof(Views.AssessmentsPage), typeof(Views.AssessmentsPage));
	}
}


