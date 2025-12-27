using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StudentLifeTracker.API.Data;
using StudentLifeTracker.API.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace StudentProgressTracker.Tests.Helpers;

public static class TestHelpers
{
    public static ApplicationDbContext CreateInMemoryDbContext(string dbName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    public static IConfiguration CreateTestConfiguration()
    {
        var config = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "TestSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!" },
            { "JwtSettings:Issuer", "TestIssuer" },
            { "JwtSettings:Audience", "TestAudience" },
            { "JwtSettings:ExpirationInMinutes", "60" },
            { "JwtSettings:RefreshTokenExpirationInDays", "7" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();
    }

    public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!, // options
            null!, // passwordHasher
            null!, // userValidators
            null!, // passwordValidators
            null!, // keyNormalizer
            null!, // errors
            null!, // services
            null!  // logger
        );
    }

    public static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager(
        UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var optionsAccessor = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();

        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            optionsAccessor.Object,
            logger.Object,
            null!, // authenticationSchemeProvider
            null!  // userConfirmation
        );
    }

    public static void SetUserContext(ControllerBase controller, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
}

