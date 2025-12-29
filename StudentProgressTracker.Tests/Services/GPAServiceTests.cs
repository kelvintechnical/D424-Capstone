using FluentAssertions;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using Xunit;

namespace StudentProgressTracker.Tests.Services;

/// <summary>
/// Unit tests for GPAService - tests pure business logic without dependencies.
/// These tests validate GPA calculation algorithms and grade conversion methods.
/// </summary>
public class GPAServiceTests
{
    private readonly GPAService _gpaService;

    public GPAServiceTests()
    {
        _gpaService = new GPAService();
    }

    [Fact]
    public void CalculateGPA_WithMultipleGrades_ReturnsWeightedAverage()
    {
        // Arrange
        var grades = new List<GradeDTO>
        {
            new GradeDTO 
            { 
                LetterGrade = "A", 
                CreditHours = 3 
            },
            new GradeDTO 
            { 
                LetterGrade = "B", 
                CreditHours = 4 
            },
            new GradeDTO 
            { 
                LetterGrade = "C", 
                CreditHours = 3 
            }
        };

        // Act
        var result = _gpaService.CalculateGPA(grades);

        // Assert
        // Expected: (4.0*3 + 3.0*4 + 2.0*3) / (3+4+3) = (12 + 12 + 6) / 10 = 3.0
        result.Should().BeApproximately(3.0, 0.01);
    }

    [Fact]
    public void CalculateGPA_WithEmptyList_ReturnsZero()
    {
        // Arrange
        var grades = new List<GradeDTO>();

        // Act
        var result = _gpaService.CalculateGPA(grades);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateGPA_WithNullList_ReturnsZero()
    {
        // Act
        var result = _gpaService.CalculateGPA(null);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void ConvertLetterToPoints_WithValidGrades_ReturnsCorrectPoints()
    {
        // Act & Assert
        _gpaService.ConvertLetterToPoints("A").Should().Be(4.0);
        _gpaService.ConvertLetterToPoints("A-").Should().Be(3.7);
        _gpaService.ConvertLetterToPoints("B+").Should().Be(3.3);
        _gpaService.ConvertLetterToPoints("B").Should().Be(3.0);
        _gpaService.ConvertLetterToPoints("B-").Should().Be(2.7);
        _gpaService.ConvertLetterToPoints("C").Should().Be(2.0);
        _gpaService.ConvertLetterToPoints("F").Should().Be(0.0);
    }

    [Fact]
    public void ConvertLetterToPoints_WithInvalidGrade_ReturnsZero()
    {
        // Act
        var result = _gpaService.ConvertLetterToPoints("INVALID");

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void ConvertLetterToPoints_WithCaseInsensitive_ReturnsCorrectPoints()
    {
        // Act & Assert
        _gpaService.ConvertLetterToPoints("a").Should().Be(4.0);
        _gpaService.ConvertLetterToPoints("A").Should().Be(4.0);
        _gpaService.ConvertLetterToPoints("b+").Should().Be(3.3);
    }

    [Fact]
    public void ConvertPercentToLetter_WithValidPercentages_ReturnsCorrectLetter()
    {
        // Act & Assert
        _gpaService.ConvertPercentToLetter(95.0).Should().Be("A");
        _gpaService.ConvertPercentToLetter(91.0).Should().Be("A-");
        _gpaService.ConvertPercentToLetter(85.0).Should().Be("B");
        _gpaService.ConvertPercentToLetter(75.0).Should().Be("C");
        _gpaService.ConvertPercentToLetter(65.0).Should().Be("D");
        _gpaService.ConvertPercentToLetter(55.0).Should().Be("F");
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithValidInputs_ReturnsCorrectNeededScore()
    {
        // Arrange
        double currentPercent = 80.0;  // Current grade: 80%
        double finalWeight = 0.3;      // Final exam worth 30%
        double targetPercent = 85.0;  // Want to achieve 85% overall

        // Act
        // Formula: needed = (target - (current × (1 - finalWeight))) / finalWeight
        // needed = (85 - (80 × 0.7)) / 0.3 = (85 - 56) / 0.3 = 96.67
        var result = _gpaService.ProjectFinalGradeNeeded(currentPercent, finalWeight, targetPercent);

        // Assert
        result.Should().BeApproximately(96.67, 0.1);
    }

    [Fact]
    public void ProjectFinalGradeNeeded_WithInvalidWeight_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => _gpaService.ProjectFinalGradeNeeded(80.0, 0.0, 85.0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Final weight must be between 0 and 1*");
    }

    [Fact]
    public void ConvertLetterToPercent_WithValidGrades_ReturnsCorrectPercent()
    {
        // Act & Assert
        _gpaService.ConvertLetterToPercent("A").Should().Be(93.0);
        _gpaService.ConvertLetterToPercent("A-").Should().Be(90.0);
        _gpaService.ConvertLetterToPercent("B+").Should().Be(87.0);
        _gpaService.ConvertLetterToPercent("B").Should().Be(83.0);
        _gpaService.ConvertLetterToPercent("C").Should().Be(73.0);
        _gpaService.ConvertLetterToPercent("F").Should().Be(0.0);
    }
}

