using MediatR;
using Xixihihi.Application.DTOs.Responses;

namespace Xixihihi.Application.Features.Users.Queries;

public class GetCurrentUserQuery : IRequest<ApiResponse<UserDto>>
{
    public Guid UserId { get; set; }
}
