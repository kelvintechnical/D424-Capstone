using StudentProgressTracker.ViewModels;

namespace StudentProgressTracker.Views;

public partial class FinancialPage : ContentPage
{
	public FinancialPage(FinancialViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is FinancialViewModel vm)
		{
			await vm.LoadDataAsync();
		}
	}

	private async void NavigateToIncome_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//IncomePage");
	}

	private async void NavigateToExpense_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//ExpensePage");
	}
}



