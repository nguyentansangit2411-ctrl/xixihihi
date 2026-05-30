using MediatR;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Exceptions;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Features.Users.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserDto>>
{
    private readonly IBaseRepository<User> _userRepository;

    public GetCurrentUserQueryHandler(IBaseRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
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

        return ApiResponse<UserDto>.SuccessResponse(userDto, "Current user retrieved successfully.");
    }
}
