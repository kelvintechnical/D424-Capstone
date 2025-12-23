using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class AssessmentsPage : ContentPage
{
	public int CourseId
	{
		get => _courseId;
		set
		{
			_courseId = value;
			_ = LoadAsync();
		}
	}

	private int _courseId;

	public AssessmentsPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<AssessmentViewModel>();
	}

	private async Task LoadAsync()
	{
		var vm = (AssessmentViewModel)BindingContext;
		await vm.LoadAssessmentsAsync(CourseId);
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}







