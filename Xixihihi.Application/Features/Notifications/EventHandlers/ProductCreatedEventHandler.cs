using MediatR;

using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Enums;
using Xixihihi.Application.Events;
using Xixihihi.Domain.Interfaces.Repositories;
using Xixihihi.Application.Interfaces;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Notifications.EventHandlers;

public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly IBaseRepository<SavedSearch> _savedSearchRepository;
    private readonly IBaseRepository<Notification> _notificationRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly IEmailQueue _emailQueue;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCreatedEventHandler(
        IBaseRepository<SavedSearch> savedSearchRepository,
        IBaseRepository<Notification> notificationRepository,
        IBaseRepository<User> userRepository,
        IEmailQueue emailQueue,
        IUnitOfWork unitOfWork)
    {
        _savedSearchRepository = savedSearchRepository;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _emailQueue = emailQueue;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        var title = notification.Title;
        var categoryId = notification.CategoryId;
        var provinceId = notification.ProvinceId;
        var price = notification.Price;

        // Find all active saved searches that match the new product
        var matchingSearches = await _savedSearchRepository.GetAsync(s => 
            s.IsActive &&
            (!s.CategoryId.HasValue || s.CategoryId == categoryId) &&
            (!s.ProvinceId.HasValue || s.ProvinceId == provinceId) &&
            (!s.MinPrice.HasValue || s.MinPrice <= price) &&
            (!s.MaxPrice.HasValue || s.MaxPrice >= price) &&
            (s.SearchTerm == null || title.Contains(s.SearchTerm)), cancellationToken);

        if (matchingSearches.Count == 0)
            return;

        var notificationsToCreate = new List<Notification>();

        // Fetch users in batch to fix N+1
        var userIds = matchingSearches.Select(s => s.UserId).Distinct().ToList();
        var users = await _userRepository.GetAsync(u => userIds.Contains(u.Id), cancellationToken);
        var userDict = users.ToDictionary(u => u.Id);

        foreach (var search in matchingSearches)
        {
            // Create a notification for the user who saved the search
            var notif = new Notification
            {
                UserId = search.UserId,
                Title = "Có sản phẩm mới phù hợp với tìm kiếm của bạn!",
                Message = $"Sản phẩm '{notification.Title}' vừa được đăng với giá {notification.Price}đ.",
                ReferenceId = notification.ProductId,
                ReferenceLink = $"/products/{notification.ProductId}",
                Type = NotificationType.NewProductMatch
            };
            
            notificationsToCreate.Add(notif);
            
            if (userDict.TryGetValue(search.UserId, out var user) && !string.IsNullOrEmpty(user.Email))
            {
                _emailQueue.Enqueue(new EmailJob(user.Email, notif.Title, notif.Message));
            }
        }

        // Add all notifications to DB
        if (notificationsToCreate.Count > 0)
        {
            await _notificationRepository.AddRangeAsync(notificationsToCreate, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

