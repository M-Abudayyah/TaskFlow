using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskFlow.Api.Controllers;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.UnitTests.Auth;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_Succeeds_WithValidAdminCredentials()
    {
        var user = CreateUser("admin-id", "admin@demo.com");
        var controller = CreateController(
            user,
            isPasswordValid: true,
            roles: new List<string> { "Admin" });

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "admin@demo.com",
            Password = "Admin@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthController.LoginResponse>(ok.Value);

        Assert.Equal("admin@demo.com", response.Email);
    }

    [Fact]
    public async Task Login_Succeeds_WithValidUserCredentials()
    {
        var user = CreateUser("user-id", "user@demo.com");
        var controller = CreateController(
            user,
            isPasswordValid: true,
            roles: new List<string> { "User" });

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "user@demo.com",
            Password = "User@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthController.LoginResponse>(ok.Value);

        Assert.Equal("user@demo.com", response.Email);
    }

    [Fact]
    public async Task Login_Fails_WhenEmailDoesNotExist()
    {
        var controller = CreateController(
            user: null,
            isPasswordValid: false,
            roles: new List<string>());

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "missing@demo.com",
            Password = "AnyPassword123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_Fails_WhenPasswordIsIncorrect()
    {
        var user = CreateUser("admin-id", "admin@demo.com");
        var controller = CreateController(
            user,
            isPasswordValid: false,
            roles: new List<string> { "Admin" });

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "admin@demo.com",
            Password = "WrongPassword"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_Returns_NonEmptyToken_OnSuccess()
    {
        var user = CreateUser("admin-id", "admin@demo.com");
        var controller = CreateController(
            user,
            isPasswordValid: true,
            roles: new List<string> { "Admin" });

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "admin@demo.com",
            Password = "Admin@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthController.LoginResponse>(ok.Value);

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_ReturnedRoles_ContainExpectedRole()
    {
        var user = CreateUser("user-id", "user@demo.com");
        var controller = CreateController(
            user,
            isPasswordValid: true,
            roles: new List<string> { "User" });

        var result = await controller.Login(new AuthController.LoginRequest
        {
            Email = "user@demo.com",
            Password = "User@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthController.LoginResponse>(ok.Value);

        Assert.Contains("User", response.Roles);
    }

    private static AppUser CreateUser(string id, string email)
    {
        return new AppUser
        {
            Id = id,
            Email = email,
            UserName = email
        };
    }

    private static AuthController CreateController(
        AppUser? user,
        bool isPasswordValid,
        List<string> roles)
    {
        var userManager = CreateUserManager(user, isPasswordValid, roles);
        var config = CreateConfiguration();

        return new AuthController(userManager.Object, config);
    }

    private static Mock<UserManager<AppUser>> CreateUserManager(
        AppUser? user,
        bool isPasswordValid,
        List<string> roles)
    {
        var store = new Mock<IUserStore<AppUser>>();
        var userManager = new Mock<UserManager<AppUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync((AppUser _, string _) => user is not null && isPasswordValid);

        userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync((AppUser _) => roles);

        return userManager;
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TaskFlow-Super-Secret-Key-For-Demo-Only-Change-This",
                ["Jwt:Issuer"] = "TaskFlow.Api",
                ["Jwt:Audience"] = "TaskFlow.Client",
                ["Jwt:DurationInMinutes"] = "60"
            })
            .Build();
    }
}
