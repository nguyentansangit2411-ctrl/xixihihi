using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

using Xixihihi.Domain.Interfaces;

namespace Xixihihi.Application.Features.Users.Commands;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<User> _userRepository;

    public UpdateUserProfileCommandHandler(IBaseRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.DisplayName = request.DisplayName;
        user.AvatarUrl = request.AvatarUrl;
        user.PhoneNumber = request.PhoneNumber;
        user.ZaloLink = request.ZaloLink;
        user.FacebookLink = request.FacebookLink;
        user.ProvinceId = request.ProvinceId;
        user.WardId = request.WardId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            ZaloLink = user.ZaloLink,
            FacebookLink = user.FacebookLink,
            Role = user.Role,
            Status = user.Status,
            ProvinceId = user.ProvinceId,
            WardId = user.WardId
        };

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User profile updated successfully.");
    }
}

