using FluentAssertions;
using Moq;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Services;
using StudentProgressTracker.ViewModels;
using Xunit;

namespace StudentProgressTracker.Tests.ViewModels;

/// <summary>
/// Unit tests for GPAViewModel - tests MVVM pattern with mocked dependencies.
/// These tests validate ViewModel behavior, command execution, and property binding
/// without requiring actual UI or network calls.
/// </summary>
public class GPAViewModelTests
{
    private readonly Mock<ApiService> _mockApiService;
    private readonly GPAService _gpaService;
    private readonly GPAViewModel _viewModel;

    public GPAViewModelTests()
    {
        _mockApiService = new Mock<ApiService>();
        _gpaService = new GPAService();
        _viewModel = new GPAViewModel(_mockApiService.Object, _gpaService);
    }

    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Assert
        _viewModel.CurrentTermGPA.Should().Be(0.0);
        _viewModel.CoursesWithGrades.Should().NotBeNull();
        _viewModel.CoursesWithGrades.Should().BeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.SelectedTermId.Should().BeNull();
        _viewModel.SelectedCourse.Should().BeNull();
        _viewModel.TargetGradeOptions.Should().HaveCount(12);
    }

    [Fact]
    public async Task LoadGPADataAsync_WithValidResponse_UpdatesProperties()
    {
        // Arrange
        var termId = 1;
        var gpaResponse = new ApiResponse<GpaResultDTO>
        {
            Success = true,
            Data = new GpaResultDTO
            {
                TermId = termId,
                GPA = 3.75,
                TotalCreditHours = 10,
                GradeCount = 3
            }
        };

        var coursesResponse = new ApiResponse<List<CourseDTO>>
        {
            Success = true,
            Data = new List<CourseDTO>
            {
                new CourseDTO
                {
                    Id = 1,
                    Title = "CS101",
                    CreditHours = 3
                }
            }
        };

        var gradesResponse = new ApiResponse<List<GradeDTO>>
        {
            Success = true,
            Data = new List<GradeDTO>
            {
                new GradeDTO
                {
                    CourseId = 1,
                    LetterGrade = "A",
                    Percentage = 95.0m,
                    CreditHours = 3
                }
            }
        };

        _mockApiService.Setup(x => x.GetTermGPAAsync(termId))
            .ReturnsAsync(gpaResponse);
        _mockApiService.Setup(x => x.GetCoursesByTermAsync(termId))
            .ReturnsAsync(coursesResponse);
        _mockApiService.Setup(x => x.GetTermGradesAsync(termId))
            .ReturnsAsync(gradesResponse);

        // Act
        await _viewModel.LoadGPADataAsync(termId);

        // Assert
        _viewModel.CurrentTermGPA.Should().Be(3.75);
        _viewModel.SelectedTermId.Should().Be(termId);
        _viewModel.CoursesWithGrades.Should().HaveCount(1);
        _viewModel.CoursesWithGrades[0].CourseTitle.Should().Be("CS101");
        _viewModel.CoursesWithGrades[0].LetterGrade.Should().Be("A");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadGPADataAsync_WithApiFailure_SetsIsLoadingToFalse()
    {
        // Arrange
        var termId = 1;
        _mockApiService.Setup(x => x.GetTermGPAAsync(termId))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _viewModel.LoadGPADataAsync(termId);

        // Assert
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.CurrentTermGPA.Should().Be(0.0);
    }

    [Fact]
    public async Task CalculateProjectionAsync_WithValidInputs_UpdatesProjectionResult()
    {
        // Arrange
        var selectedCourse = new CourseGradeInfo
        {
            CourseId = 1,
            CourseTitle = "CS101",
            Percentage = 80.0m,
            CreditHours = 3
        };
        _viewModel.SelectedCourse = selectedCourse;
        _viewModel.FinalWeightInput = "0.3";
        _viewModel.SelectedTargetGrade = "A";

        var projectionResponse = new ApiResponse<GradeProjectionDTO>
        {
            Success = true,
            Data = new GradeProjectionDTO
            {
                CourseId = 1,
                CurrentGrade = 80.0,
                FinalWeight = 0.3,
                TargetGrade = "A",
                IsAchievable = true,
                NeededOnFinal = 96.67
            }
        };

        _mockApiService.Setup(x => x.GetGradeProjectionAsync(
            It.IsAny<int>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<string>()))
            .ReturnsAsync(projectionResponse);

        // Act
        await _viewModel.CalculateProjectionCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ProjectionResult.Should().Contain("96.7");
        _viewModel.ProjectionResult.Should().Contain("A");
        _viewModel.IsCalculatingProjection.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateProjectionAsync_WithNoSelectedCourse_DoesNotCallApi()
    {
        // Arrange
        _viewModel.SelectedCourse = null;
        _viewModel.FinalWeightInput = "0.3";

        // Act
        await _viewModel.CalculateProjectionCommand.ExecuteAsync(null);

        // Assert
        _mockApiService.Verify(x => x.GetGradeProjectionAsync(
            It.IsAny<int>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CalculateProjectionAsync_WithInvalidFinalWeight_DoesNotCallApi()
    {
        // Arrange
        var selectedCourse = new CourseGradeInfo
        {
            CourseId = 1,
            CourseTitle = "CS101",
            Percentage = 80.0m
        };
        _viewModel.SelectedCourse = selectedCourse;
        _viewModel.FinalWeightInput = "invalid";

        // Act
        await _viewModel.CalculateProjectionCommand.ExecuteAsync(null);

        // Assert
        _mockApiService.Verify(x => x.GetGradeProjectionAsync(
            It.IsAny<int>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OnSelectedCourseChanged_WithCourseHavingGrade_UpdatesCurrentGradeInput()
    {
        // Arrange
        var course = new CourseGradeInfo
        {
            CourseId = 1,
            CourseTitle = "CS101",
            Percentage = 85.5m
        };

        // Act
        _viewModel.SelectedCourse = course;

        // Assert
        _viewModel.CurrentGradeInput.Should().Be("85.5");
    }

    [Fact]
    public void OnSelectedCourseChanged_WithCourseWithoutGrade_ClearsCurrentGradeInput()
    {
        // Arrange
        var course = new CourseGradeInfo
        {
            CourseId = 1,
            CourseTitle = "CS101",
            Percentage = null
        };

        // Act
        _viewModel.SelectedCourse = course;

        // Assert
        _viewModel.CurrentGradeInput.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportTranscriptAsync_WithValidData_CallsApiService()
    {
        // Arrange
        var csvBytes = System.Text.Encoding.UTF8.GetBytes("CSV,Data,Here");
        _mockApiService.Setup(x => x.DownloadTranscriptCsvAsync())
            .ReturnsAsync(csvBytes);

        // Act
        await _viewModel.ExportTranscriptCommand.ExecuteAsync(null);

        // Assert
        _mockApiService.Verify(x => x.DownloadTranscriptCsvAsync(), Times.Once);
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ExportTermGpaAsync_WithNoSelectedTerm_DoesNotCallApi()
    {
        // Arrange
        _viewModel.SelectedTermId = null;

        // Act
        await _viewModel.ExportTermGpaCommand.ExecuteAsync(null);

        // Assert
        _mockApiService.Verify(x => x.DownloadGpaReportCsvAsync(
            It.IsAny<int>()), Times.Never);
    }
}

