using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

[QueryProperty(nameof(TermId), "termId")]
public partial class CourseListPage : ContentPage
{
	public int TermId
	{
		get => _termId;
		set
		{
			_termId = value;
			_ = LoadAsync();
		}
	}

	private int _termId;

	public CourseListPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<CourseListViewModel>();
	}

	private async Task LoadAsync()
	{
		var vm = (CourseListViewModel)BindingContext;

		// Load the term details
		var db = ServiceHelper.GetRequiredService<Services.DatabaseService>();
		var term = await db.GetTermAsync(TermId);
		if (term != null)
		{
			vm.SetCurrentTerm(term);
		}

		await vm.LoadCoursesAsync(TermId);
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}







