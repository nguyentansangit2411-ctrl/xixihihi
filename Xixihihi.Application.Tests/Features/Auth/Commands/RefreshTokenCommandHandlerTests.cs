using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xixihihi.Application.Features.Auth.Commands;
using Xixihihi.Application.Interfaces;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IBaseRepository<User>> _mockUserRepository;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockJwtService = new Mock<IJwtService>();
        _mockUserRepository = new Mock<IBaseRepository<User>>();
        _handler = new RefreshTokenCommandHandler(_mockJwtService.Object, _mockUserRepository.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_InvalidOrExpiredToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "invalid" };
        
        // Setup returns null
        _mockUserRepository.Setup(repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "valid_refresh" };
        var existingUser = new User 
        { 
            Id = Guid.NewGuid(), 
            Email = "test@gmail.com", 
            DisplayName = "Test User", 
            Role = UserRole.Buyer,
            RefreshToken = "valid_refresh",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };
        
        _mockUserRepository.Setup(repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
            
        _mockJwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("new_access_token");
        _mockJwtService.Setup(s => s.GenerateRefreshToken()).Returns("new_refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("new_access_token");
        result.Data.RefreshToken.Should().Be("new_refresh_token");
        
    }
}

