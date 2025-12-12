using StudentProgressTracker.ViewModels;
using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Views;

public partial class CategoryPage : ContentPage
{
	public CategoryPage(CategoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is CategoryViewModel vm)
		{
			await vm.LoadCategoriesAsync();
		}
	}

	private void EditCategory_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is CategoryDTO category && BindingContext is CategoryViewModel vm)
		{
			vm.EditCategoryCommand.Execute(category);
		}
	}

	private void DeleteCategory_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button && button.CommandParameter is CategoryDTO category && BindingContext is CategoryViewModel vm)
		{
			vm.DeleteCategoryCommand.Execute(category);
		}
	}
}



