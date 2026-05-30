using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Xixihihi.API.Tests.Controllers;

public class NotificationControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public NotificationControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
                new Claim(JwtRegisteredClaimNames.Email, "user@xixihihi.local"),
                new Claim(ClaimTypes.Role, "Buyer"),
                new Claim("name", "Test User")
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
    public async Task GetNotifications_ReturnsOkResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = GenerateUserToken(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/notifications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MarkAsRead_ExistingNotification_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Xixihihi.Infrastructure.Data.AppDbContext>();
        
        var userId = Guid.NewGuid();
        db.Users.Add(new Xixihihi.Domain.Entities.User { Id = userId, Email = "user@xixihihi.local", DisplayName = "User" });
        
        var notificationId = Guid.NewGuid();
        db.Set<Xixihihi.Domain.Entities.Notification>().Add(new Xixihihi.Domain.Entities.Notification
        {
            Id = notificationId,
            UserId = userId,
            Title = "Test Notif",
            Message = "Test Message",
            IsRead = false
        });
        await db.SaveChangesAsync();

        var token = GenerateUserToken(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PatchAsync($"/api/notifications/{notificationId}/read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
