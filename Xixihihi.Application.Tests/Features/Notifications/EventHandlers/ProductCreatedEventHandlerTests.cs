using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xixihihi.Application.Events;
using Xixihihi.Application.Features.Notifications.EventHandlers;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.Notifications.EventHandlers;

public class ProductCreatedEventHandlerTests
{
    private readonly Mock<IBaseRepository<SavedSearch>> _mockSavedSearchRepository;
    private readonly Mock<IBaseRepository<Notification>> _mockNotificationRepository;
    private readonly Mock<IBaseRepository<User>> _mockUserRepository;
    private readonly Mock<Xixihihi.Application.Interfaces.IEmailQueue> _mockEmailQueue;
    private readonly ProductCreatedEventHandler _handler;

    public ProductCreatedEventHandlerTests()
    {
        _mockSavedSearchRepository = new Mock<IBaseRepository<SavedSearch>>();
        _mockNotificationRepository = new Mock<IBaseRepository<Notification>>();
        _mockUserRepository = new Mock<IBaseRepository<User>>();
        _mockEmailQueue = new Mock<Xixihihi.Application.Interfaces.IEmailQueue>();
        _handler = new ProductCreatedEventHandler(
            _mockSavedSearchRepository.Object, 
            _mockNotificationRepository.Object,
            _mockUserRepository.Object,
            _mockEmailQueue.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_ProductMatchesSavedSearch_CreatesNotification()
    {
        // Arrange
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            Title = "Iphone 13 Pro Max",
            Price = 15000000,
            CategoryId = Guid.NewGuid(),
            ProvinceId = Guid.NewGuid()
        };

        var savedSearches = new List<SavedSearch>
        {
            new SavedSearch // Khớp (Match)
            {
                UserId = Guid.NewGuid(),
                SearchTerm = "iphone 13",
                MaxPrice = 20000000,
                IsActive = true
            },
            new SavedSearch // Không khớp do giá
            {
                UserId = Guid.NewGuid(),
                SearchTerm = "iphone 13",
                MaxPrice = 10000000,
                IsActive = true
            }
        };

        _mockSavedSearchRepository.Setup(repo => repo.GetAsync(
            It.IsAny<Expression<Func<SavedSearch, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedSearches);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        // Chỉ lưu 1 thông báo cho SavedSearch đầu tiên
        _mockNotificationRepository.Verify(repo => repo.AddAsync(It.Is<Notification>(n => n.Type == NotificationType.NewProductMatch), It.IsAny<CancellationToken>()), Times.Once);
    }
}

