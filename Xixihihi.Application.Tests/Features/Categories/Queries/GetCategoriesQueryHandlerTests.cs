using AutoMapper;
using FluentAssertions;
using Moq;
using Xixihihi.Application.Features.Categories.Queries;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Categories.Queries;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<IBaseRepository<Category>> _mockCategoryRepository;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _mockCategoryRepository = new Mock<IBaseRepository<Category>>();
        _cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

        _handler = new GetCategoriesQueryHandler(_mockCategoryRepository.Object, _cache);
    }

    [Fact]
    public async Task Handle_ReturnsListOfCategories()
    {
        // Arrange
        var query = new GetCategoriesQuery();
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Cat 1" },
            new Category { Id = Guid.NewGuid(), Name = "Cat 2" }
        };

        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Id = categories[0].Id, Name = "Cat 1" },
            new CategoryDto { Id = categories[1].Id, Name = "Cat 2" }
        };

        _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);

        _mockCategoryRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
