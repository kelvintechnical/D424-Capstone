using System.Globalization;
using Microsoft.Maui.Controls;

namespace StudentProgressTracker.Helpers;

public class SearchTypeToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string searchType)
        {
            return searchType == "Courses";
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

