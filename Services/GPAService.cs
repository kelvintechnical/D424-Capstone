using StudentLifeTracker.Shared.DTOs;

namespace StudentProgressTracker.Services;

public class GPAService
{
    public double CalculateGPA(List<GradeDTO> grades)
    {
        if (grades == null || grades.Count == 0)
            return 0.0;

        double totalPoints = 0;
        int totalCreditHours = 0;

        foreach (var grade in grades)
        {
            var points = ConvertLetterToPoints(grade.LetterGrade);
            totalPoints += points * grade.CreditHours;
            totalCreditHours += grade.CreditHours;
        }

        if (totalCreditHours == 0)
            return 0.0;

        return totalPoints / totalCreditHours;
    }

    public double ProjectFinalGradeNeeded(double currentPercent, double finalWeight, double targetPercent)
    {
        if (finalWeight <= 0 || finalWeight > 1)
            throw new ArgumentException("Final weight must be between 0 and 1.");

        // Formula: needed = (target - (current × (1 - finalWeight))) / finalWeight
        var needed = (targetPercent - (currentPercent * (1 - finalWeight))) / finalWeight;
        return needed;
    }

    public double ConvertLetterToPoints(string letterGrade)
    {
        return letterGrade.ToUpper() switch
        {
            "A+" or "A" => 4.0,
            "A-" => 3.7,
            "B+" => 3.3,
            "B" => 3.0,
            "B-" => 2.7,
            "C+" => 2.3,
            "C" => 2.0,
            "C-" => 1.7,
            "D+" => 1.3,
            "D" => 1.0,
            "D-" => 0.7,
            "F" => 0.0,
            _ => 0.0
        };
    }

    public string ConvertPercentToLetter(double percent)
    {
        return percent switch
        {
            >= 93.0 => "A",
            >= 90.0 => "A-",
            >= 87.0 => "B+",
            >= 83.0 => "B",
            >= 80.0 => "B-",
            >= 77.0 => "C+",
            >= 73.0 => "C",
            >= 70.0 => "C-",
            >= 67.0 => "D+",
            >= 63.0 => "D",
            >= 60.0 => "D-",
            _ => "F"
        };
    }

    public double ConvertLetterToPercent(string letterGrade)
    {
        return letterGrade.ToUpper() switch
        {
            "A+" or "A" => 93.0,
            "A-" => 90.0,
            "B+" => 87.0,
            "B" => 83.0,
            "B-" => 80.0,
            "C+" => 77.0,
            "C" => 73.0,
            "C-" => 70.0,
            "D+" => 67.0,
            "D" => 63.0,
            "D-" => 60.0,
            "F" => 0.0,
            _ => -1.0
        };
    }
}

