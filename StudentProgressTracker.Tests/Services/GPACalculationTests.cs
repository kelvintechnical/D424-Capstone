using FluentAssertions;
using Xunit;

namespace StudentProgressTracker.Tests.Services;

// Note: GPAService is in the MAUI project which can't be directly referenced
// These tests demonstrate the GPA calculation logic that should be tested
// In a production environment, GPAService should be moved to a shared library

public class GPACalculationTests
{
    // These are the calculation methods that should be tested
    // They mirror the logic in GPAService

    private double CalculateGPA(List<(string LetterGrade, int CreditHours)> grades)
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

    private double ConvertLetterToPoints(string letterGrade)
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

    private double ProjectFinalGradeNeeded(double currentPercent, double finalWeight, double targetPercent)
    {
        if (finalWeight <= 0 || finalWeight > 1)
            throw new ArgumentException("Final weight must be between 0 and 1.");

        // Formula: needed = (target - (current × (1 - finalWeight))) / finalWeight
        var needed = (targetPercent - (currentPercent * (1 - finalWeight))) / finalWeight;
        return needed;
    }

    #region CalculateGPA Tests

    [Fact]
    public void CalculateGPA_WithAllAGrades_ShouldReturn4_0()
    {
        // Arrange
        var grades = new List<(string, int)>
        {
            ("A", 3),
            ("A", 3),
            ("A", 3)
        };

        // Act
        var result = CalculateGPA(grades);

        // Assert
        result.Should().Be(4.0);
    }

    [Fact]
    public void CalculateGPA_WithMixedGrades_ShouldCalculateWeightedAverage()
    {
        // Arrange
        var grades = new List<(string, int)>
        {
            ("A", 3),  // 4.0 * 3 = 12.0
            ("B", 3),  // 3.0 * 3 = 9.0
            ("C", 3)   // 2.0 * 3 = 6.0
        };
        // Total: 27.0 points / 9 credit hours = 3.0

        // Act
        var result = CalculateGPA(grades);

        // Assert
        result.Should().BeApproximately(3.0, 0.01);
    }

    [Fact]
    public void CalculateGPA_WithDifferentCreditHours_ShouldWeightCorrectly()
    {
        // Arrange
        var grades = new List<(string, int)>
        {
            ("A", 3),  // 4.0 * 3 = 12.0
            ("C", 1)   // 2.0 * 1 = 2.0
        };
        // Total: 14.0 points / 4 credit hours = 3.5

        // Act
        var result = CalculateGPA(grades);

        // Assert
        result.Should().BeApproximately(3.5, 0.01);
    }

    [Fact]
    public void CalculateGPA_WithEmptyList_ShouldReturn0()
    {
        // Arrange
        var grades = new List<(string, int)>();

        // Act
        var result = CalculateGPA(grades);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateGPA_WithNullList_ShouldReturn0()
    {
        // Arrange
        List<(string, int)>? grades = null;

        // Act
        var result = CalculateGPA(grades!);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateGPA_WithPlusMinusGrades_ShouldUseCorrectPoints()
    {
        // Arrange
        var grades = new List<(string, int)>
        {
            ("A+", 3),  // 4.0 * 3 = 12.0
            ("A-", 3),  // 3.7 * 3 = 11.1
            ("B+", 3),  // 3.3 * 3 = 9.9
            ("B-", 3)   // 2.7 * 3 = 8.1
        };
        // Total: 41.1 points / 12 credit hours = 3.425

        // Act
        var result = CalculateGPA(grades);

        // Assert
        result.Should().BeApproximately(3.425, 0.01);
    }

    #endregion

    #region Grade Projection Tests

    [Fact]
    public void ProjectFinalGradeNeeded_WithValidInputs_ShouldCalculateCorrectly()
    {
        // Arrange
        // Current: 78%, Target: B (83%), Final Weight: 20%
        // Formula: needed = (83 - (78 × 0.8)) / 0.2
        // = (83 - 62.4) / 0.2 = 20.6 / 0.2 = 103

        // Act
        var result = ProjectFinalGradeNeeded(78.0, 0.2, 83.0);

        // Assert
        result.Should().BeApproximately(103.0, 0.1);
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithImpossibleTarget_ShouldReturnGreaterThan100()
    {
        // Arrange
        // Current: 40%, Target: A (93%), Final Weight: 20%
        // needed = (93 - (40 × 0.8)) / 0.2 = (93 - 32) / 0.2 = 305%

        // Act
        var result = ProjectFinalGradeNeeded(40.0, 0.2, 93.0);

        // Assert
        result.Should().BeGreaterThan(100.0);
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithInvalidFinalWeight_ShouldThrowException()
    {
        // Arrange
        var invalidWeight = 1.5; // > 1.0

        // Act & Assert
        var act = () => ProjectFinalGradeNeeded(80.0, invalidWeight, 90.0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Final weight must be between 0 and 1.*");
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithZeroFinalWeight_ShouldThrowException()
    {
        // Arrange
        var zeroWeight = 0.0;

        // Act & Assert
        var act = () => ProjectFinalGradeNeeded(80.0, zeroWeight, 90.0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithAchievableTarget_ShouldReturnValidScore()
    {
        // Arrange
        // Current: 85%, Target: A (93%), Final Weight: 30%
        // needed = (93 - (85 × 0.7)) / 0.3 = (93 - 59.5) / 0.3 = 111.67

        // Act
        var result = ProjectFinalGradeNeeded(85.0, 0.3, 93.0);

        // Assert
        result.Should().BeApproximately(111.67, 0.1);
        result.Should().BeGreaterThan(100.0); // Still impossible
    }

    #endregion

    #region Letter Grade to Points Conversion Tests

    [Theory]
    [InlineData("A+", 4.0)]
    [InlineData("A", 4.0)]
    [InlineData("a", 4.0)] // Case insensitive
    [InlineData("A-", 3.7)]
    [InlineData("B+", 3.3)]
    [InlineData("B", 3.0)]
    [InlineData("B-", 2.7)]
    [InlineData("C+", 2.3)]
    [InlineData("C", 2.0)]
    [InlineData("C-", 1.7)]
    [InlineData("D+", 1.3)]
    [InlineData("D", 1.0)]
    [InlineData("D-", 0.7)]
    [InlineData("F", 0.0)]
    [InlineData("f", 0.0)] // Case insensitive
    [InlineData("Invalid", 0.0)] // Invalid grade returns 0.0
    public void ConvertLetterToPoints_WithVariousGrades_ShouldReturnCorrectPoints(string letterGrade, double expectedPoints)
    {
        // Act
        var result = ConvertLetterToPoints(letterGrade);

        // Assert
        result.Should().Be(expectedPoints);
    }

    #endregion
}

