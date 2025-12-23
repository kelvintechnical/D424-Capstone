using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class CourseDetailPage : ContentPage
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

	public CourseDetailPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<CourseDetailViewModel>();
	}

	private async Task LoadAsync()
	{
		var vm = (CourseDetailViewModel)BindingContext;
		await vm.LoadCourseAsync(CourseId);
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}







