using System.Globalization;
using Microsoft.Maui.Controls;

namespace StudentProgressTracker.Helpers;

/// <summary>
/// Converter that returns true if a string is not null or empty, false otherwise.
/// Used for showing/hiding error labels in XAML.
/// </summary>
public class IsStringNotEmptyConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string str)
		{
			return !string.IsNullOrWhiteSpace(str);
		}
		return false;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException("One-way binding only");
	}
}

