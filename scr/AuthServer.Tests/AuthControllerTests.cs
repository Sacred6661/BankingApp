using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Controllers;
using AuthServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;

    public AuthControllerTests()
    {
        // Мок UserStore - для конструктора UserManager
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();

        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _webHostEnvironmentMock.Setup(env => env.EnvironmentName).Returns("Development");
        _webHostEnvironmentMock.Setup(env => env.ContentRootPath).Returns(@"C:\MyApp\");
        _webHostEnvironmentMock.Setup(env => env.WebRootPath).Returns(@"C:\MyApp\wwwroot");
        _webHostEnvironmentMock.Setup(env => env.ApplicationName).Returns("MyApp");
    }

    // 1. Register t
    [Fact]
    public async Task Register_ReturnsOk_WhenUserCreatedSuccessfully()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var message = root.GetProperty("message").GetString();

        Assert.Equal("User registered successfully", message);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserCreationFails()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "fail@example.com",
            Password = "weak"
        };

        var errors = new List<IdentityError> { new IdentityError { Description = "Password too weak" } };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.Register(registerRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(errors, badRequestResult.Value);
    }

    // 2. Login Test
    [Fact]
    public async Task Login_ReturnsToken_WhenResponseIsSuccessful()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        string tokenResponseContent = "{\"access_token\":\"dummy_token\",\"expires_in\":3600}";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(tokenResponseContent)
        };

        var httpClient = CreateMockHttpClient(responseMessage);

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientId"]).Returns("client_id");
        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientAppSecret"]).Returns("client_secret");
        _configurationMock.SetupGet(x => x["IdentitySecrets:Scope"]).Returns("openid profile roles api_gateway account_service transaction_service history_service offline_access");

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Equal(tokenResponseContent, contentResult.Content);
    }

    [Fact]
    public async Task Login_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "fail@example.com",
            Password = "badpassword"
        };

        string errorResponseContent = "{\"error\":\"invalid_grant\"}";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorResponseContent)
        };

        var httpClient = CreateMockHttpClient(responseMessage);

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientId"]).Returns("client_id");
        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientAppSecret"]).Returns("client_secret");
        _configurationMock.SetupGet(x => x["IdentitySecrets:Scope"]).Returns("openid profile");

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        Assert.Equal(errorResponseContent, objectResult.Value);
    }

    // 3. RefreshToken test
    [Fact]
    public async Task RefreshToken_ReturnsToken_WhenResponseIsSuccessful()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest
        {
            RefreshToken = "refresh_token_value"
        };

        string tokenResponseContent = "{\"access_token\":\"new_dummy_token\",\"expires_in\":3600}";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(tokenResponseContent)
        };

        var httpClient = CreateMockHttpClient(responseMessage);

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientId"]).Returns("client_id");
        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientAppSecret"]).Returns("client_secret");

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.RefreshToken(refreshTokenRequest);

        // Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Equal(tokenResponseContent, contentResult.Content);
    }

    [Fact]
    public async Task RefreshToken_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest
        {
            RefreshToken = "bad_refresh_token"
        };

        string errorResponseContent = "{\"error\":\"invalid_token\"}";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorResponseContent)
        };

        var httpClient = CreateMockHttpClient(responseMessage);

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientId"]).Returns("client_id");
        _configurationMock.SetupGet(x => x["IdentitySecrets:ClientAppSecret"]).Returns("client_secret");

        var controller = CreateControllerWithContext();

        // Act
        var result = await controller.RefreshToken(refreshTokenRequest);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        Assert.Equal(errorResponseContent, objectResult.Value);
    }

    // 4. Additional tests

    [Fact]
    public async Task Register_CallsUserManagerCreateAsync_Once()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateControllerWithContext();

        // Act
        await controller.Register(registerRequest);

        // Assert
        _userManagerMock.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => u.Email == "test@example.com"), "Password123!"), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }


    // --- additional methods ---

    // create moqed HttpClient with a given answer
    private HttpClient CreateMockHttpClient(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(responseMessage)
           .Verifiable();

        return new HttpClient(handlerMock.Object);
    }

    // Creates controller with moqs and set HttpContext for URL building
    private AuthController CreateControllerWithContext()
    {
        var controller = new AuthController(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _userManagerMock.Object,
            _webHostEnvironmentMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        return controller;
    }
}
