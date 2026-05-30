using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xixihihi.Application.Features.Categories.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Categories.Commands;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Category>> _mockCategoryRepository;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<IBaseRepository<Category>>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockCache = new Mock<IMemoryCache>();

        _handler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object, _mockUow.Object, _mockCache.Object);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeleteCategoryCommand { Id = Guid.NewGuid() };
        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Category with ID {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesCategoryAndReturnsTrue()
    {
        // Arrange
        var command = new DeleteCategoryCommand { Id = Guid.NewGuid() };
        var categoryEntity = new Category { Id = command.Id, Name = "Sports" };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _mockCategoryRepository.Verify(repo => repo.UpdateAsync(categoryEntity, It.IsAny<CancellationToken>()), Times.Once);
    }
}

