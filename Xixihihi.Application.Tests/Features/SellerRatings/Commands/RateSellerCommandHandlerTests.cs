using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xixihihi.Application.Features.SellerRatings.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.SellerRatings.Commands;

public class RateSellerCommandHandlerTests
{
    private readonly Mock<IBaseRepository<SellerRating>> _mockSellerRatingRepository;
    private readonly Mock<IBaseRepository<User>> _mockUserRepository;
    private readonly RateSellerCommandHandler _handler;

    public RateSellerCommandHandlerTests()
    {
        _mockSellerRatingRepository = new Mock<IBaseRepository<SellerRating>>();
        _mockUserRepository = new Mock<IBaseRepository<User>>();
        _handler = new RateSellerCommandHandler(_mockSellerRatingRepository.Object, _mockUserRepository.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_ReviewerIsSeller_ThrowsBusinessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RateSellerCommand { ReviewerId = userId, SellerId = userId, Rating = 5 };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("You cannot rate yourself.");
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesRating()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var command = new RateSellerCommand { ReviewerId = reviewerId, SellerId = sellerId, Rating = 4, Comment = "Good" };

        var seller = new User { Id = sellerId, DisplayName = "Seller" };
        var reviewer = new User { Id = reviewerId, DisplayName = "Reviewer" };

        _mockUserRepository.Setup(repo => repo.GetByIdAsync(sellerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seller);
        _mockUserRepository.Setup(repo => repo.GetByIdAsync(reviewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewer);
            
        // Setup No existing rating
        _mockSellerRatingRepository.Setup(repo => repo.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<SellerRating, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SellerRating?)null);

        _mockSellerRatingRepository.Setup(repo => repo.AddAsync(It.IsAny<SellerRating>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SellerRating r, CancellationToken ct) => { r.Id = Guid.NewGuid(); return r; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Rating.Should().Be(4);
        result.Data.Comment.Should().Be("Good");
    }
}

