using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Controllers;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using StudentLifeTracker.API.Services;
using StudentLifeTracker.Shared.DTOs;
using StudentLifeTracker.Tests.Helpers;
using Xunit;

namespace StudentLifeTracker.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);

        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _configurationMock = new Mock<IConfiguration>();

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Test_Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Test123!";
        var user = new ApplicationUser
        {
            Id = "user123",
            Email = email,
            UserName = email,
            Name = "Test User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("test-token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("test-token", response.Data.Token);
    }

    [Fact]
    public async Task Test_Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var user = new ApplicationUser
        {
            Id = "user123",
            Email = email,
            UserName = email
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(unauthorizedResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Test_Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = string.Empty,
            Password = "Test123!"
        };

        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Test_Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = string.Empty
        };

        _controller.ModelState.AddModelError("Password", "Password is required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Test_Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Test123!",
            Name = "New User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<ApplicationUser>()))
            .Returns("test-token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task Test_Register_WithExistingEmail_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Test123!",
            Name = "Existing User"
        };

        var existingUser = new ApplicationUser
        {
            Id = "user123",
            Email = request.Email,
            UserName = request.Email
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AuthResponse>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message ?? string.Empty);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

