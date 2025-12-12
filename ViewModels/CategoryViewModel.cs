using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using System.Collections.ObjectModel;

namespace StudentProgressTracker.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
	private readonly FinancialService _financialService;

	[ObservableProperty] private ObservableCollection<CategoryDTO> categories = new();
	[ObservableProperty] private CategoryDTO? selectedCategory;
	[ObservableProperty] private bool isLoading;
	[ObservableProperty] private bool isEditing;
	[ObservableProperty] private string name = string.Empty;

	public CategoryViewModel(FinancialService financialService)
	{
		_financialService = financialService;
	}

	[RelayCommand]
	public async Task LoadCategoriesAsync()
	{
		IsLoading = true;
		try
		{
			var categoryList = await _financialService.GetCategoriesAsync();
			Categories.Clear();
			foreach (var category in categoryList)
			{
				Categories.Add(category);
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public void StartNewCategory()
	{
		SelectedCategory = null;
		Name = string.Empty;
		IsEditing = true;
	}

	[RelayCommand]
	public void EditCategory(CategoryDTO category)
	{
		SelectedCategory = category;
		Name = category.Name;
		IsEditing = true;
	}

	[RelayCommand]
	public async Task SaveCategoryAsync()
	{
		if (string.IsNullOrWhiteSpace(Name))
		{
			await Application.Current.MainPage.DisplayAlert("Validation", "Category name is required", "OK");
			return;
		}

		IsLoading = true;
		try
		{
			var category = new CategoryDTO
			{
				Id = SelectedCategory?.Id ?? 0,
				Name = Name.Trim()
			};

			bool success;
			if (SelectedCategory != null)
			{
				success = await _financialService.UpdateCategoryAsync(SelectedCategory.Id, category);
			}
			else
			{
				success = await _financialService.CreateCategoryAsync(category);
			}

			if (success)
			{
				IsEditing = false;
				await LoadCategoriesAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Category saved successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to save category", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save category: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	public void CancelEdit()
	{
		IsEditing = false;
		SelectedCategory = null;
	}

	[RelayCommand]
	public async Task DeleteCategoryAsync(CategoryDTO category)
	{
		var confirm = await Application.Current.MainPage.DisplayAlert(
			"Confirm Delete",
			$"Are you sure you want to delete category '{category.Name}'? This will fail if there are expenses using this category.",
			"Yes",
			"No");

		if (!confirm) return;

		IsLoading = true;
		try
		{
			var success = await _financialService.DeleteCategoryAsync(category.Id);
			if (success)
			{
				await LoadCategoriesAsync();
				await Application.Current.MainPage.DisplayAlert("Success", "Category deleted successfully", "OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete category. It may have associated expenses.", "OK");
			}
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete category: {ex.Message}", "OK");
		}
		finally
		{
			IsLoading = false;
		}
	}
}



