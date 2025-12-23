using StudentProgressTracker.ViewModels;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Views;

public partial class ExpensePage : ContentPage
{
	public ExpensePage(ExpenseViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is ExpenseViewModel vm)
		{
			await vm.LoadDataAsync();
		}
	}

	private void EditExpense_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is ExpenseDTO expense && BindingContext is ExpenseViewModel vm)
		{
			vm.EditExpenseCommand.Execute(expense);
		}
	}

	private void CategoryPicker_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (sender is Picker picker && picker.SelectedItem is CategoryDTO category && BindingContext is ExpenseViewModel vm)
		{
			vm.SelectedCategory = category;
		}
	}

	private async void LoginToolbarItem_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//LoginPage");
	}
}

