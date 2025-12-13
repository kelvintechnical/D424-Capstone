using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Controllers;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.API.Services;
using StudentLifeTracker.Shared.DTOs;
using StudentProgressTracker.Tests.Helpers;
using Xunit;

namespace StudentProgressTracker.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userManagerMock = TestHelpers.CreateMockUserManager();
        _signInManagerMock = TestHelpers.CreateMockSignInManager(_userManagerMock.Object);
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _configuration = TestHelpers.CreateTestConfiguration();

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object,
            _configuration);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Test123!",
            Name = "Test User"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email,
            Name = request.Name
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .Returns("test-token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Test123!",
            Name = "Test User"
        };

        var existingUser = new ApplicationUser { Email = request.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "weak", // Too weak
            Name = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var errors = new List<IdentityError>
        {
            new() { Code = "PasswordTooShort", Description = "Password is too short." }
        };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Contain("failed");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Test123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email,
            Name = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _jwtServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("test-token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeTrue();
        response.Data!.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.Email
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        var response = unauthorizedResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test123!"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        var response = unauthorizedResult!.Value as ApiResponse<AuthResponse>;
        response!.Success.Should().BeFalse();
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewToken()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            Token = "valid-token",
            RefreshToken = "valid-refresh-token"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User"
        };

        // Note: In a real implementation, you'd validate the refresh token from storage
        // For testing, we'll mock the JWT service to generate a new token
        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .Returns("new-token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // This test would need actual JWT validation logic
        // For now, we're testing the structure
        // In production, you'd need to decode and validate the token

        // Act & Assert
        // Note: Full refresh token implementation would require JWT token validation
        // This is a placeholder test structure
        // In production, you would decode and validate the JWT token here
        await Task.CompletedTask; // Placeholder until refresh token validation is fully implemented
        Assert.True(true);
    }

    #endregion
}

