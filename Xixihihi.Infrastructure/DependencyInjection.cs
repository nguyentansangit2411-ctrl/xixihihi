using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Interfaces.Repositories;
using Xixihihi.Infrastructure.Data;
using Xixihihi.Infrastructure.Repositories;
using Xixihihi.Infrastructure.Services;

namespace Xixihihi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!);

        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISellerRatingRepository, SellerRatingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<Xixihihi.Domain.Interfaces.IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        
        // Use CloudinaryService instead of LocalStorageService
        services.AddScoped<IStorageService, CloudinaryService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEmailQueue, EmailQueue>();
        services.AddHostedService<EmailBackgroundService>();

        return services;
    }
}
