using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProgressTracker.Services;

public class AlertService : IAlertService
{
    public async Task ShowAlertAsync(string title, string message, string cancel)
    {
        if (Application.Current?.Windows?.Count > 0)
        {
            await Application.Current.Windows[0].Page.DisplayAlert(title, message, cancel);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        if (Application.Current?.Windows?.Count > 0)
        {
            return await Application.Current.Windows[0].Page.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}