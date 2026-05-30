using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xixihihi.Application.DTOs.Requests;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.API.Tests.Controllers;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private string GenerateUserToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this_is_a_very_long_secret_key_for_xixihihi_development_only");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "buyer@xixihihi.local"),
                new Claim(ClaimTypes.Role, "Buyer"),
                new Claim("name", "Buyer User")
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = "Xixihihi",
            Audience = "XixihihiClient",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Usually GetCurrentUser with Token would fail if User is not in DB (since our handler gets from DB).
    // We would need to mock DB state or let it fail with 404/Exception if our handler throws NotFoundException.
    // In our Integration test, empty InMemoryDb means the user doesn't exist.
    // So we expect 500 or 404 depending on how ExceptionHandlingMiddleware handles NotFoundException.
    [Fact]
    public async Task GetCurrentUser_WithValidToken_UserNotInDb_ReturnsNotFound()
    {
        // Arrange
        var token = GenerateUserToken(Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users/me");

        // Assert
        // ExceptionHandlingMiddleware maps NotFoundException to 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
