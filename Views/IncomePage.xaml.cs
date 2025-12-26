using StudentProgressTracker.ViewModels;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Views;

public partial class IncomePage : ContentPage
{
	public IncomePage(IncomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is IncomeViewModel vm)
		{
			await vm.LoadIncomesAsync();
		}
	}

	private void EditIncome_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is IncomeDTO income && BindingContext is IncomeViewModel vm)
		{
			vm.EditIncomeCommand.Execute(income);
		}
	}

	private void DeleteIncome_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is IncomeDTO income && BindingContext is IncomeViewModel vm)
		{
			vm.DeleteIncomeCommand.Execute(income);
		}
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}

