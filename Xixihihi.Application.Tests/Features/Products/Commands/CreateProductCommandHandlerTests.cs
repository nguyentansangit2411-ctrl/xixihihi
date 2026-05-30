using FluentAssertions;
using AutoMapper;
using Moq;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Application.Features.Products.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();

        _handler = new CreateProductCommandHandler(
            _mockProductRepository.Object,
            new Mock<IBaseRepository<Category>>().Object,
            new Mock<IBaseRepository<Province>>().Object,
            new Mock<IBaseRepository<Ward>>().Object,
            _mockMapper.Object,
            new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesProductAndReturnsDto()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            SellerId = Guid.NewGuid(),
            Title = "Shoes",
            Description = "Nike Shoes",
            Price = 1000000,
            IsNegotiable = true,
            Condition = ProductCondition.New,
            TransactionType = TransactionType.ShipCOD,
            CategoryId = Guid.NewGuid(),
            ProvinceId = Guid.NewGuid(),
            WardId = Guid.NewGuid()
        };

        var productWithDetails = new Product
        {
            Id = Guid.NewGuid(),
            SellerId = command.SellerId,
            Title = command.Title,
            Description = command.Description,
            Price = command.Price,
            IsNegotiable = command.IsNegotiable,
            Condition = command.Condition,
            Status = ProductStatus.Draft,
            TransactionType = command.TransactionType,
            CategoryId = command.CategoryId,
            ProvinceId = command.ProvinceId,
            WardId = command.WardId,
            CreatedAt = DateTime.UtcNow,
            Seller = new User
            {
                Id = command.SellerId,
                Email = "seller@test.com",
                DisplayName = "Seller",
                Role = UserRole.Seller,
                Status = UserStatus.Active
            }
        };

        _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken c) => p);

        _mockProductRepository.Setup(repo => repo.GetProductWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productWithDetails);

        var handler = new CreateProductCommandHandler(
            _mockProductRepository.Object,
            Mock.Of<IBaseRepository<Category>>(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()) == Task.FromResult<Category?>(new Category { Id = command.CategoryId })),
            Mock.Of<IBaseRepository<Province>>(repo => repo.GetByIdAsync(command.ProvinceId, It.IsAny<CancellationToken>()) == Task.FromResult<Province?>(new Province { Id = command.ProvinceId })),
            Mock.Of<IBaseRepository<Ward>>(repo => repo.GetByIdAsync(command.WardId, It.IsAny<CancellationToken>()) == Task.FromResult<Ward?>(new Ward { Id = command.WardId })),
            _mockMapper.Object,
            new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Shoes");
        result.Data.Status.Should().Be(ProductStatus.Draft);
        result.Data.Seller.Should().NotBeNull();
        result.Data.Seller.DisplayName.Should().Be("Seller");

        _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockProductRepository.Verify(repo => repo.GetProductWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
