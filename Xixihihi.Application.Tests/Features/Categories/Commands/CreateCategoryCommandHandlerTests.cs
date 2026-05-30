using AutoMapper;
using FluentAssertions;
using Moq;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Features.Categories.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Categories.Commands;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Category>> _mockCategoryRepository;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<IBaseRepository<Category>>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockCache = new Mock<IMemoryCache>();

        _handler = new CreateCategoryCommandHandler(_mockCategoryRepository.Object, _mockUow.Object, _mockCache.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesCategoryAndReturnsDto()
    {
        // Arrange
        var command = new CreateCategoryCommand 
        { 
            Name = "Sports", 
            Description = "Sport items" 
        };

        var categoryEntity = new Category 
        { 
            Id = Guid.NewGuid(), 
            Name = "Sports", 
            Description = "Sport items" 
        };

        var categoryDto = new CategoryDto 
        { 
            Id = categoryEntity.Id, 
            Name = "Sports", 
            Description = "Sport items" 
        };

        _mockCategoryRepository.Setup(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Sports");

        _mockCategoryRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

