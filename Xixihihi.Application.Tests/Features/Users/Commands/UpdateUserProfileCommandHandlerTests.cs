using FluentAssertions;
using Moq;
using Xixihihi.Application.Features.Users.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Users.Commands;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IBaseRepository<User>> _mockUserRepository;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IBaseRepository<User>>();

        _handler = new UpdateUserProfileCommandHandler(_mockUserRepository.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateUserProfileCommand { UserId = Guid.NewGuid(), DisplayName = "Test" };
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesUserAndReturnsDto()
    {
        // Arrange
        var command = new UpdateUserProfileCommand 
        { 
            UserId = Guid.NewGuid(), 
            DisplayName = "Updated Name",
            PhoneNumber = "0123456789"
        };
        
        var userEntity = new User 
        { 
            Id = command.UserId, 
            DisplayName = "Old Name",
            Email = "test@example.com"
        };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.DisplayName.Should().Be("Updated Name");
        result.Data.PhoneNumber.Should().Be("0123456789");
        
        userEntity.DisplayName.Should().Be("Updated Name");
        userEntity.PhoneNumber.Should().Be("0123456789");

        _mockUserRepository.Verify(repo => repo.UpdateAsync(userEntity, It.IsAny<CancellationToken>()), Times.Once);
    }
}

