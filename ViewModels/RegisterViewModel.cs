using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;

namespace StudentProgressTracker.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public RegisterViewModel(ApiService apiService, IAlertService alertService)
    {
        _apiService = apiService;
        _alertService = alertService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters long.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        IsLoading = true;

        try
        {
            var request = new RegisterRequest
            {
                Email = Email.Trim(),
                Password = Password,
                Name = Name.Trim()
            };

            var response = await _apiService.RegisterAsync(request);

            if (response.Success && response.Data != null)
            {
                await _alertService.ShowAlertAsync("Success", "Registration successful! You can now login.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                var errors = response.Errors != null && response.Errors.Any()
                    ? string.Join("\n", response.Errors)
                    : response.Message ?? "Registration failed.";
                ErrorMessage = errors;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Registration error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
