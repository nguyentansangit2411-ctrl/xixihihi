using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xixihihi.Application.Features.Auth.Commands;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.API.Tests.Controllers;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GoogleLogin_WithValidToken_ReturnsTokens()
    {
        // Arrange
        var request = new LoginWithGoogleCommand { Token = "valid_token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/google", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GoogleLogin_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginWithGoogleCommand { Token = "invalid_token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/google", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var loginReq = new LoginWithGoogleCommand { Token = "valid_token" };
        var loginRes = await _client.PostAsJsonAsync("/api/auth/google", loginReq);
        var loginResult = await loginRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        var refreshToken = loginResult!.Data!.RefreshToken;

        var refreshReq = new RefreshTokenCommand { RefreshToken = refreshToken };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenCommand { RefreshToken = "invalid_refresh_token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
