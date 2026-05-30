using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xixihihi.Infrastructure.Data;

namespace Xixihihi.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "this_is_a_very_long_secret_key_for_xixihihi_development_only");
        builder.UseSetting("Jwt:Issuer", "Xixihihi");
        builder.UseSetting("Jwt:Audience", "XixihihiClient");
        builder.UseSetting("AllowedOrigins", "http://localhost:3000");
        builder.UseSetting("OAuth:Google:ClientId", "test-client-id");
        
        builder.ConfigureServices(services =>
        {
            // Remove the app's AppDbContext registration completely
            var descriptors = services.Where(d => d.ServiceType.Namespace != null && d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore")).ToList();
            foreach (var d in descriptors)
            {
                services.Remove(d);
            }

            // Add AppDbContext using an in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            // Mock IGoogleAuthService
            services.RemoveAll(typeof(Xixihihi.Application.Interfaces.IGoogleAuthService));
            var mockGoogleAuth = new Moq.Mock<Xixihihi.Application.Interfaces.IGoogleAuthService>();
            mockGoogleAuth.Setup(x => x.ValidateTokenAsync("valid_token", Moq.It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult<Xixihihi.Application.Interfaces.GoogleUserInfo?>(new Xixihihi.Application.Interfaces.GoogleUserInfo { ProviderKey = "12345", Email = "test@example.com", Name = "Test User", PictureUrl = "https://example.com/pic.jpg" }));
            mockGoogleAuth.Setup(x => x.ValidateTokenAsync("invalid_token", Moq.It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.FromResult<Xixihihi.Application.Interfaces.GoogleUserInfo?>(null));
            services.AddSingleton(mockGoogleAuth.Object);
        });
    }
}
