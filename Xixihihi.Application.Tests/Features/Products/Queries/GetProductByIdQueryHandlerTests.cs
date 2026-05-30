using FluentAssertions;
using Moq;
using Xixihihi.Application.Features.Products.Queries;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Products.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();

        _handler = new GetProductByIdQueryHandler(_mockProductRepository.Object);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetProductByIdQuery { Id = Guid.NewGuid() };
        _mockProductRepository.Setup(repo => repo.GetProductWithDetailsAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with ID {query.Id} not found.");
    }

    [Fact]
    public async Task Handle_ProductFound_ReturnsDto()
    {
        // Arrange
        var query = new GetProductByIdQuery { Id = Guid.NewGuid() };
        var product = new Product
        {
            Id = query.Id,
            SellerId = Guid.NewGuid(),
            Title = "Shoes",
            Price = 1000000,
            CreatedAt = DateTime.UtcNow,
            Seller = new User
            {
                Id = Guid.NewGuid(),
                Email = "seller@test.com",
                DisplayName = "Seller",
                Role = UserRole.Seller,
                Status = UserStatus.Active
            }
        };

        _mockProductRepository.Setup(repo => repo.GetProductWithDetailsAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(query.Id);
        result.Data.Title.Should().Be("Shoes");
        result.Data.Seller.DisplayName.Should().Be("Seller");
    }
}
