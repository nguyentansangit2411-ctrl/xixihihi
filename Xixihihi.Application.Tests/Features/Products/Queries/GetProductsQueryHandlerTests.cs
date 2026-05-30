using FluentAssertions;
using Moq;
using Xixihihi.Application.Features.Products.Queries;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Products.Queries;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();

        _handler = new GetProductsQueryHandler(_mockProductRepository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedProducts()
    {
        // Arrange
        var query = new GetProductsQuery { Page = 1, PageSize = 10 };
        
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Title = "Shoes 1",
                Price = 1000000,
                Status = ProductStatus.Active,
                Seller = new User { Id = Guid.NewGuid(), DisplayName = "Seller 1" }
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Title = "Shoes 2",
                Price = 2000000,
                Status = ProductStatus.Active,
                Seller = new User { Id = Guid.NewGuid(), DisplayName = "Seller 2" }
            }
        };

        _mockProductRepository.Setup(repo => repo.GetProductsAsync(
            query.Search, query.CategoryId, query.ProvinceId, query.WardId,
            query.Condition, query.TransactionType, query.MinPrice, query.MaxPrice,
            query.IsNegotiable, query.Brand, query.SellerId, query.Status, 
            query.SortBy, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
        result.Data.Items.First().Title.Should().Be("Shoes 1");
    }
}
