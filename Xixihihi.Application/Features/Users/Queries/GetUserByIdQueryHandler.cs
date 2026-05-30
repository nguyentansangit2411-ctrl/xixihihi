using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IBaseRepository<User> _userRepository;

    public GetUserByIdQueryHandler(IBaseRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found.");
        }

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

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User retrieved successfully.");
    }
}
