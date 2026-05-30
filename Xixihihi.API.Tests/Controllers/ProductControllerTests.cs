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
using Xixihihi.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Xixihihi.API.Tests.Controllers;

public class ProductControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private string GenerateSellerToken(Guid sellerId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this_is_a_very_long_secret_key_for_xixihihi_development_only");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, sellerId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "seller@xixihihi.local"),
                new Claim(ClaimTypes.Role, "Seller"),
                new Claim("name", "Seller User")
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
    public async Task GetProducts_ReturnsOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<ProductDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithValidToken_CreatesProduct()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Xixihihi.Infrastructure.Data.AppDbContext>();
        
        var categoryId = Guid.NewGuid();
        db.Categories.Add(new Xixihihi.Domain.Entities.Category { Id = categoryId, Name = "Test Category" });
        
        var provinceId = Guid.NewGuid();
        db.Provinces.Add(new Xixihihi.Domain.Entities.Province { Id = provinceId, Name = "HCM", Code = "HCM" });
        
        var wardId = Guid.NewGuid();
        db.Wards.Add(new Xixihihi.Domain.Entities.Ward { Id = wardId, ProvinceId = provinceId, Name = "Q1" });

        var sellerId = Guid.NewGuid();
        db.Users.Add(new Xixihihi.Domain.Entities.User { Id = sellerId, Email = "seller@xixihihi.local", DisplayName = "Seller" });
        await db.SaveChangesAsync();

        var request = new CreateProductRequest
        {
            Title = "Running Shoes",
            Description = "Good condition running shoes",
            Price = 500000,
            IsNegotiable = true,
            Condition = ProductCondition.LikeNew,
            TransactionType = TransactionType.ShipCOD,
            CategoryId = categoryId,
            ProvinceId = provinceId,
            WardId = wardId
        };
        
        var token = GenerateSellerToken(sellerId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Running Shoes");
    }
}
