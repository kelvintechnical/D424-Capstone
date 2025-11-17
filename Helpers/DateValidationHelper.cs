namespace StudentProgressTracker.Helpers;

/// <summary>
/// Helper class for validating dates within 1 year of today's date.
/// </summary>
public static class DateValidationHelper
{
	/// <summary>
	/// Validates if a date is within 1 year (past or future) of today's date.
	/// </summary>
	/// <param name="date">The date to validate</param>
	/// <returns>A validation result containing whether the date is valid and an error message if invalid</returns>
	public static DateValidationResult ValidateDate(DateTime date)
	{
		var today = DateTime.Today;
		var oneYearAgo = today.AddYears(-1);
		var oneYearFromNow = today.AddYears(1);

		if (date < oneYearAgo || date > oneYearFromNow)
		{
			var minDate = oneYearAgo.ToString("d");
			var maxDate = oneYearFromNow.ToString("d");
			var errorMessage = $"Date must be within 1 year of today. Allowed range: {minDate} to {maxDate}";
			
			return new DateValidationResult
			{
				IsValid = false,
				ErrorMessage = errorMessage,
				MinAllowedDate = oneYearAgo,
				MaxAllowedDate = oneYearFromNow
			};
		}

		return new DateValidationResult { IsValid = true };
	}

	/// <summary>
	/// Gets the allowed date range (1 year before and after today).
	/// </summary>
	/// <returns>A tuple containing the minimum and maximum allowed dates</returns>
	public static (DateTime minDate, DateTime maxDate) GetAllowedDateRange()
	{
		var today = DateTime.Today;
		return (today.AddYears(-1), today.AddYears(1));
	}
}

/// <summary>
/// Result of date validation containing validation status and error information.
/// </summary>
public class DateValidationResult
{
	public bool IsValid { get; set; }
	public string ErrorMessage { get; set; } = string.Empty;
	public DateTime MinAllowedDate { get; set; }
	public DateTime MaxAllowedDate { get; set; }
}

