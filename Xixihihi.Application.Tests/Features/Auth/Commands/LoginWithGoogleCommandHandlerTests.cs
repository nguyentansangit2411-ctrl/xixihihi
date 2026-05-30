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

public class LoginWithGoogleCommandHandlerTests
{
    private readonly Mock<IGoogleAuthService> _mockGoogleAuthService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IBaseRepository<User>> _mockUserRepository;
    private readonly LoginWithGoogleCommandHandler _handler;

    public LoginWithGoogleCommandHandlerTests()
    {
        _mockGoogleAuthService = new Mock<IGoogleAuthService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockUserRepository = new Mock<IBaseRepository<User>>();

        _handler = new LoginWithGoogleCommandHandler(
            _mockGoogleAuthService.Object,
            _mockJwtService.Object,
            _mockUserRepository.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new LoginWithGoogleCommand { Token = "invalid_token" };
        _mockGoogleAuthService.Setup(s => s.ValidateTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid Google token.");
    }

    [Fact]
    public async Task Handle_ValidToken_UserExists_ReturnsAuthResponse()
    {
        // Arrange
        var command = new LoginWithGoogleCommand { Token = "valid_token" };
        var googleUserInfo = new GoogleUserInfo { Email = "test@gmail.com", Name = "Test User", ProviderKey = "123" };
        var existingUser = new User { Id = Guid.NewGuid(), Email = "test@gmail.com", DisplayName = "Test User", Role = UserRole.Buyer };
        
        _mockGoogleAuthService.Setup(s => s.ValidateTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUserInfo);
            
        _mockUserRepository.Setup(repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
            
        _mockJwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _mockJwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");
        result.Data.User.Email.Should().Be("test@gmail.com");
        
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ValidToken_NewUser_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var command = new LoginWithGoogleCommand { Token = "valid_token" };
        var googleUserInfo = new GoogleUserInfo { Email = "new@gmail.com", Name = "New User", ProviderKey = "456" };
        
        _mockGoogleAuthService.Setup(s => s.ValidateTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUserInfo);
            
        _mockUserRepository.Setup(repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
            
        _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken c) => u);
            
        _mockJwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _mockJwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.User.Email.Should().Be("new@gmail.com");
        
        _mockUserRepository.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Email == "new@gmail.com"), It.IsAny<CancellationToken>()), Times.Once);
    }
}

