using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace StudentProgressTracker.Helpers;

public class ResultTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string resultType)
        {
            return resultType switch
            {
                "Term" => Color.FromArgb("#4A90E2"), // Blue
                "Course" => Color.FromArgb("#50C878"), // Green
                _ => Color.FromArgb("#808080") // Gray
            };
        }
        return Color.FromArgb("#808080");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("One-way binding only");
    }
}

