using StudentProgressTracker.Helpers;
using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

[QueryProperty(nameof(TermId), "termId")]
public partial class TermDetailPage : ContentPage
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

	public TermDetailPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<TermDetailViewModel>();
	}

	private async Task LoadAsync()
	{
		var vm = (TermDetailViewModel)BindingContext;
		await vm.LoadTermAsync(TermId);
	}
}

